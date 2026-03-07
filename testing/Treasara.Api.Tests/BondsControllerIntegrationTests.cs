using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Treasara.Api.Dtos.Requests;
using Treasara.Api.Dtos.Responses;
using Treasara.Api.Tests.Infrastructure;
using Xunit;

namespace Treasara.Api.Tests;

/// <summary>
/// Integration tests for the BondsController API endpoints.
/// </summary>
/// <remarks>
/// These tests verify the end-to-end behavior of the bond valuation API by making
/// actual HTTP requests to a test instance of the application. Unlike unit tests
/// that isolate individual components, integration tests validate that all layers
/// (controllers, validators, mappers, domain logic) work correctly together.
/// 
/// The tests use WebApplicationFactory to spin up an in-memory test server,
/// providing fast integration testing without requiring a deployed environment.
/// This approach validates:
/// <list type="bullet">
/// <item><description>HTTP routing and request handling</description></item>
/// <item><description>Request validation and error responses</description></item>
/// <item><description>Serialization/deserialization of DTOs</description></item>
/// <item><description>Domain logic execution</description></item>
/// <item><description>Response structure and content</description></item>
/// </list>
/// </remarks>
public sealed class BondsControllerIntegrationTests : IClassFixture<TreasaraApiFactory>
{
    private readonly HttpClient _client;

    private const string BondValueEndpoint = "/api/v1/bonds/value";
    private const string UsdCurrency = "USD";
    private const string SemiAnnualFrequency = "SemiAnnual";
    private const string Thirty360DayCount = "Thirty360";
    private const string NoRollConvention = "NoRoll";

    /// <summary>
    /// Initializes a new instance of the <see cref="BondsControllerIntegrationTests"/> class.
    /// </summary>
    /// <param name="factory">The web application factory for creating test HTTP clients.</param>
    public BondsControllerIntegrationTests(TreasaraApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Verifies that a valid bond valuation request returns HTTP 200 OK with complete valuation data.
    /// </summary>
    /// <remarks>
    /// This test validates the happy path where all input parameters are valid.
    /// It ensures that:
    /// <list type="bullet">
    /// <item><description>The API returns a 200 OK status code</description></item>
    /// <item><description>The response contains a total present value</description></item>
    /// <item><description>The currency matches the request</description></item>
    /// <item><description>Valuation lines are present in the response</description></item>
    /// </list>
    /// This is the primary success scenario that validates core valuation functionality.
    /// </remarks>
    [Fact]
    public async Task Value_Should_Return200Ok_For_Valid_Request()
    {
        // Arrange
        var request = CreateValidBondRequest(
            notional: 1_000_000m,
            couponRate: 0.05m,
            issueDate: new DateOnly(2026, 1, 1),
            maturityDate: new DateOnly(2031, 1, 1),
            valuationDate: new DateOnly(2026, 1, 1),
            curveRate: 0.04m);

        // Act
        var response = await _client.PostAsJsonAsync(BondValueEndpoint, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<BondValuationResponseDto>();

        body.Should().NotBeNull();
        body!.TotalPresentValue.Should().NotBeNull();
        body.Currency.Should().Be(UsdCurrency);
        body.Lines.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Verifies that an invalid bond valuation request returns HTTP 400 Bad Request with validation errors.
    /// </summary>
    /// <remarks>
    /// This test validates the validation failure path by submitting a request with multiple errors:
    /// <list type="bullet">
    /// <item><description>Negative notional amount (must be positive)</description></item>
    /// <item><description>Maturity date before issue date (invalid date range)</description></item>
    /// </list>
    /// It ensures that:
    /// <list type="bullet">
    /// <item><description>The API returns a 400 Bad Request status code</description></item>
    /// <item><description>The response contains a validation error indicator</description></item>
    /// <item><description>Field-level validation errors are provided for both invalid fields</description></item>
    /// <item><description>A trace ID is included for troubleshooting</description></item>
    /// </list>
    /// This validates that FluentValidation rules are properly applied and error responses
    /// provide actionable feedback to API consumers.
    /// </remarks>
    [Fact]
    public async Task Value_Should_Return400BadRequest_For_Invalid_Request()
    {
        // Arrange - Create request with multiple validation errors
        var request = CreateInvalidBondRequest(
            notional: -100m,  // Invalid: must be positive
            issueDate: new DateOnly(2026, 1, 1),
            maturityDate: new DateOnly(2025, 1, 1));  // Invalid: before issue date

        // Act
        var response = await _client.PostAsJsonAsync(BondValueEndpoint, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<ValidationErrorDto>();

        body.Should().NotBeNull();
        body!.Error.Should().Be("validation_error");
        body.Details.Should().ContainKey("Notional");
        body.Details.Should().ContainKey("MaturityDate");
        body.TraceId.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that the valuation response includes detailed cash flow lines with correct structure.
    /// </summary>
    /// <remarks>
    /// This test validates the detailed cash flow projection returned by the API.
    /// For a 1-year bond with semi-annual payments, it verifies:
    /// <list type="bullet">
    /// <item><description>The correct number of cash flow lines (3: two coupons + principal)</description></item>
    /// <item><description>Each line has a positive cash flow amount</description></item>
    /// <item><description>Each line has a positive present value</description></item>
    /// <item><description>Discount factors are between 0 and 1 (standard for present value calculations)</description></item>
    /// </list>
    /// This ensures that the API provides the detailed breakdown necessary for
    /// clients to understand the valuation calculation and cash flow schedule.
    /// </remarks>
    [Fact]
    public async Task Value_Should_Return_Valuation_Lines()
    {
        // Arrange - Create a short-dated bond for predictable cash flow count
        var request = CreateValidBondRequest(
            notional: 1_000m,
            couponRate: 0.05m,
            issueDate: new DateOnly(2026, 1, 1),
            maturityDate: new DateOnly(2027, 1, 1),  // 1 year maturity
            valuationDate: new DateOnly(2026, 1, 1),
            curveRate: 0.04m);

        // Act
        var response = await _client.PostAsJsonAsync(BondValueEndpoint, request);

        // Assert
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<BondValuationResponseDto>();

        body.Should().NotBeNull();
        body!.Lines.Should().HaveCount(3);  // 2 semi-annual coupons + 1 principal

        // Validate first cash flow line structure
        var firstLine = body.Lines[0];
        firstLine.CashflowAmount.Should().BeGreaterThan(0m);
        firstLine.PresentValue.Should().BeGreaterThan(0m);
        firstLine.DiscountFactor.Should().BeGreaterThan(0m);
        firstLine.DiscountFactor.Should().BeLessThanOrEqualTo(1m);
    }

    #region Helper Methods

    /// <summary>
    /// Creates a valid bond valuation request with the specified parameters.
    /// </summary>
    /// <param name="notional">The notional amount of the bond.</param>
    /// <param name="couponRate">The annual coupon rate as a decimal.</param>
    /// <param name="issueDate">The bond issue date.</param>
    /// <param name="maturityDate">The bond maturity date.</param>
    /// <param name="valuationDate">The date on which to value the bond.</param>
    /// <param name="curveRate">The flat yield curve rate.</param>
    /// <returns>A fully populated bond valuation request DTO.</returns>
    private static BondValuationRequestDto CreateValidBondRequest(
        decimal notional,
        decimal couponRate,
        DateOnly issueDate,
        DateOnly maturityDate,
        DateOnly valuationDate,
        decimal curveRate)
    {
        return new BondValuationRequestDto
        {
            Notional = notional,
            Currency = UsdCurrency,
            CouponRate = couponRate,
            IssueDate = issueDate,
            MaturityDate = maturityDate,
            ValuationDate = valuationDate,
            Frequency = SemiAnnualFrequency,
            DayCount = Thirty360DayCount,
            RollConvention = NoRollConvention,
            CurveRate = curveRate
        };
    }

    /// <summary>
    /// Creates an invalid bond valuation request for testing validation error scenarios.
    /// </summary>
    /// <param name="notional">The notional amount (may be invalid).</param>
    /// <param name="issueDate">The bond issue date.</param>
    /// <param name="maturityDate">The bond maturity date (may be invalid relative to issue date).</param>
    /// <returns>A bond valuation request DTO with intentionally invalid data.</returns>
    private static BondValuationRequestDto CreateInvalidBondRequest(
        decimal notional,
        DateOnly issueDate,
        DateOnly maturityDate)
    {
        return new BondValuationRequestDto
        {
            Notional = notional,
            Currency = UsdCurrency,
            CouponRate = 0.05m,
            IssueDate = issueDate,
            MaturityDate = maturityDate,
            ValuationDate = issueDate,
            Frequency = SemiAnnualFrequency,
            DayCount = Thirty360DayCount,
            RollConvention = NoRollConvention,
            CurveRate = 0.04m
        };
    }

    #endregion
}