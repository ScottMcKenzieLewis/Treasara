using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using System.Net;
using System.Net.Http.Json;
using Treasara.Api.Dtos.Requests;
using Treasara.Api.Dtos.Responses;
using Treasara.Api.Tests.Infrastructure;

namespace Treasara.Api.Tests.Properties;

/// <summary>
/// Property-based tests for the BondsController using FsCheck.
/// </summary>
/// <remarks>
/// This test suite uses property-based testing to verify mathematical invariants
/// and behavioral properties of bond valuation that should hold true for a wide
/// range of input values. Unlike example-based tests that validate specific scenarios,
/// property-based tests generate many random inputs to verify that certain properties
/// (mathematical relationships or business rules) always hold.
/// 
/// Property-based testing is particularly valuable for financial calculations where
/// mathematical properties like linearity, commutativity, or monotonicity should be
/// preserved across all valid inputs.
/// </remarks>
public sealed class BondsControllerIntegrationPropertyTests : IClassFixture<TreasaraApiFactory>
{
    private readonly HttpClient _client;

    private const string BondValueEndpoint = "/api/v1/bonds/value";
    private const string UsdCurrency = "USD";
    private const string SemiAnnualFrequency = "SemiAnnual";
    private const string Thirty360DayCount = "Thirty360";
    private const string NoRollConvention = "NoRoll";
    
    private const int MinNotional = 1;
    private const int MaxNotional = 10_000;
    private const int MinMultiplier = 1;
    private const int MaxMultiplier = 20;
    private const decimal LinearityTolerance = 0.0001m;

    /// <summary>
    /// Initializes a new instance of the <see cref="BondsControllerIntegrationPropertyTests"/> class.
    /// </summary>
    /// <param name="factory">The web application factory for creating test HTTP clients.</param>
    public BondsControllerIntegrationPropertyTests(TreasaraApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Property test verifying that bond valuation scales linearly with notional amount.
    /// </summary>
    /// <param name="baseAmount">The base notional amount (will be clamped to valid range).</param>
    /// <param name="multiplier">The scaling factor to apply to the notional (will be clamped to valid range).</param>
    /// <returns>
    /// A property that passes if the present value of a bond with notional (baseAmount × multiplier)
    /// equals the present value of a bond with notional (baseAmount) multiplied by the multiplier,
    /// within a small tolerance for numerical precision.
    /// </returns>
    /// <remarks>
    /// This test verifies the fundamental mathematical property of bond valuation: linearity with
    /// respect to notional amount. For any valid bond parameters, doubling the notional should
    /// exactly double the present value, tripling should triple it, etc.
    /// 
    /// Mathematical property being tested:
    /// <para>PV(n × m) = PV(n) × m</para>
    /// where n is the base notional and m is the multiplier.
    /// 
    /// The test process:
    /// <list type="number">
    /// <item><description>FsCheck generates random base amounts and multipliers</description></item>
    /// <item><description>Values are clamped to reasonable ranges (1-10,000 for base, 1-20 for multiplier)</description></item>
    /// <item><description>Two identical bonds are created except for notional (one with n, one with n×m)</description></item>
    /// <item><description>Both bonds are valued using the API</description></item>
    /// <item><description>Verifies that PV(n×m) ≈ PV(n)×m within tolerance of 0.0001</description></item>
    /// </list>
    /// 
    /// The tolerance is kept very small (0.0001) because valuation is exactly linear in notional
    /// with no approximations involved. Any deviation indicates a potential calculation error.
    /// 
    /// This property is run with 10 random test cases (MaxTest = 10) to balance test coverage
    /// with execution speed in the CI/CD pipeline.
    /// 
    /// Enhanced error reporting:
    /// The test uses FsCheck's <c>Prop.Label</c> to provide detailed failure messages including:
    /// <list type="bullet">
    /// <item><description>HTTP status codes if requests fail</description></item>
    /// <item><description>Expected vs. actual present values</description></item>
    /// <item><description>The difference between expected and actual values</description></item>
    /// </list>
    /// This makes it easier to diagnose failures when they occur.
    /// </remarks>    
    [Property(MaxTest = 10)]
    [Trait("Category", "Integration")]
    public Property Value_Should_Scale_With_Notional(int baseAmount, int multiplier)
    {
        // Clamp inputs to reasonable ranges to ensure validation passes
        var clampedBaseAmount = ClampToValidRange(baseAmount, MinNotional, MaxNotional);
        var clampedMultiplier = ClampToValidRange(multiplier, MinMultiplier, MaxMultiplier);

        // Create requests with different notional amounts
        var baseRequest = CreateStandardBondRequest(clampedBaseAmount);
        var scaledRequest = CreateStandardBondRequest(clampedBaseAmount * clampedMultiplier);

        // Execute valuations for both bonds
        var baseResponse = PostValuationRequest(baseRequest);
        var scaledResponse = PostValuationRequest(scaledRequest);

        // Verify both requests succeeded
        if (!baseResponse.IsSuccessStatusCode || !scaledResponse.IsSuccessStatusCode)
        {
            return Prop.Label(
                false,
                $"Expected HTTP 200 but got {baseResponse.StatusCode} and {scaledResponse.StatusCode}");
        }

        // Deserialize responses
        var baseValuation = DeserializeResponse(baseResponse);
        var scaledValuation = DeserializeResponse(scaledResponse);

        // Verify responses contain present value data
        if (baseValuation?.TotalPresentValue is null || scaledValuation?.TotalPresentValue is null)
        {
            return Prop.Label(false, "TotalPresentValue was null in API response");
        }

        // Verify linearity property: PV(n×m) ≈ PV(n)×m
        return VerifyLinearityProperty(
            baseValuation.TotalPresentValue.Value,
            scaledValuation.TotalPresentValue.Value,
            clampedMultiplier);
    }

    #region Helper Methods

    /// <summary>
    /// Clamps a value to a specified range by taking the absolute value and constraining it.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The clamped value within [min, max].</returns>
    /// <remarks>
    /// This method ensures that randomly generated values from FsCheck fall within
    /// valid ranges for the API. It takes the absolute value first to handle negative
    /// inputs, then clamps to the specified range.
    /// </remarks>
    private static int ClampToValidRange(int value, int min, int max)
    {
        return Math.Max(min, Math.Min(Math.Abs(value), max));
    }

    /// <summary>
    /// Creates a standard bond valuation request with the specified notional amount.
    /// </summary>
    /// <param name="notional">The notional amount for the bond.</param>
    /// <returns>A bond valuation request DTO with standardized parameters.</returns>
    /// <remarks>
    /// Uses fixed parameters for all properties except notional to isolate the
    /// linearity property being tested. The bond has:
    /// <list type="bullet">
    /// <item><description>1-year maturity from 2026-01-01 to 2027-01-01</description></item>
    /// <item><description>5% coupon rate</description></item>
    /// <item><description>Semi-annual payment frequency (2 coupon payments + principal)</description></item>
    /// <item><description>Thirty360 day count convention</description></item>
    /// <item><description>4% discount curve rate</description></item>
    /// <item><description>No roll convention adjustments</description></item>
    /// </list>
    /// This standardization ensures that any differences in valuation are due solely
    /// to the notional amount, not other parameter variations.
    /// </remarks>
    private static BondValuationRequestDto CreateStandardBondRequest(int notional)
    {
        return new BondValuationRequestDto
        {
            Notional = notional,
            Currency = UsdCurrency,
            CouponRate = 0.05m,
            IssueDate = new DateOnly(2026, 1, 1),
            MaturityDate = new DateOnly(2027, 1, 1),
            ValuationDate = new DateOnly(2026, 1, 1),
            Frequency = SemiAnnualFrequency,
            DayCount = Thirty360DayCount,
            RollConvention = NoRollConvention,
            CurveRate = 0.04m
        };
    }

    /// <summary>
    /// Posts a bond valuation request to the API and returns the response.
    /// </summary>
    /// <param name="request">The bond valuation request to post.</param>
    /// <returns>The HTTP response message from the API.</returns>
    /// <remarks>
    /// Uses synchronous waiting (GetAwaiter().GetResult()) because FsCheck property tests
    /// cannot be async. This is acceptable in test code where we control the execution context.
    /// </remarks>
    private HttpResponseMessage PostValuationRequest(BondValuationRequestDto request)
    {
        return _client
            .PostAsJsonAsync(BondValueEndpoint, request)
            .GetAwaiter()
            .GetResult();
    }

    /// <summary>
    /// Deserializes a bond valuation response from an HTTP response message.
    /// </summary>
    /// <param name="response">The HTTP response containing the bond valuation.</param>
    /// <returns>The deserialized bond valuation response DTO, or null if deserialization fails.</returns>
    /// <remarks>
    /// Returns null on deserialization failure to allow the calling code to check for null
    /// and provide an appropriate error message via Prop.Label.
    /// </remarks>
    private static BondValuationResponseDto? DeserializeResponse(HttpResponseMessage response)
    {
        return response.Content
            .ReadFromJsonAsync<BondValuationResponseDto>()
            .GetAwaiter()
            .GetResult();
    }

    /// <summary>
    /// Verifies the linearity property: PV(n×m) ≈ PV(n)×m.
    /// </summary>
    /// <param name="basePresentValue">The present value for the base notional.</param>
    /// <param name="scaledPresentValue">The present value for the scaled notional.</param>
    /// <param name="multiplier">The multiplier applied to the notional.</param>
    /// <returns>
    /// A property that passes if the scaled present value matches the expected value
    /// within the defined tolerance, with detailed failure messages.
    /// </returns>
    /// <remarks>
    /// The tolerance is kept very small (0.0001) because bond valuation is mathematically
    /// linear in notional with no approximations. Any significant deviation suggests a bug
    /// in the calculation logic.
    /// 
    /// Failure messages include:
    /// <list type="bullet">
    /// <item><description>Expected present value (base PV × multiplier)</description></item>
    /// <item><description>Actual present value from the scaled bond</description></item>
    /// <item><description>The absolute difference between expected and actual</description></item>
    /// </list>
    /// </remarks>
    private static Property VerifyLinearityProperty(
        decimal basePresentValue,
        decimal scaledPresentValue,
        int multiplier)
    {
        var expected = basePresentValue * multiplier;
        var actual = scaledPresentValue;
        var difference = Math.Abs(actual - expected);

        return Prop.Label(
            difference < LinearityTolerance,
            $"PV scaling failed. Expected={expected:N4}, Actual={actual:N4}, Difference={difference:N6}");
    }

    #endregion
}