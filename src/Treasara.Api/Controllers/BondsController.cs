using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Treasara.Api.Dtos.Requests;
using Treasara.Api.Dtos.Responses;
using Treasara.Domain;

namespace Treasara.Api.Controllers;

/// <summary>
/// API controller for bond valuation operations.
/// </summary>
/// <remarks>
/// Provides endpoints for calculating bond valuations, including present value,
/// accrued interest, and cash flow projections based on market yield curves.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class BondsController : ControllerBase
{
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="BondsController"/> class.
    /// </summary>
    /// <param name="mapper">The AutoMapper instance for DTO mappings.</param>
    public BondsController(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// Calculates the valuation of a bond based on the provided parameters.
    /// </summary>
    /// <param name="request">The bond valuation request containing bond characteristics and market data.</param>
    /// <returns>A detailed bond valuation including present value, accrued interest, and cash flows.</returns>
    /// <response code="200">Returns the calculated bond valuation.</response>
    /// <response code="400">If the request contains invalid parameters or unsupported values.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/v1/bonds/value
    ///     {
    ///         "notional": 1000000,
    ///         "currency": "USD",
    ///         "couponRate": 0.05,
    ///         "issueDate": "2024-01-01",
    ///         "maturityDate": "2034-01-01",
    ///         "valuationDate": "2026-03-06",
    ///         "frequency": "SEMIANNUAL",
    ///         "dayCount": "ACTUAL360",
    ///         "rollConvention": "NOROLL",
    ///         "curveRate": 0.045
    ///     }
    /// 
    /// Supported values:
    /// - Currency: USD, EUR, GBP
    /// - Frequency: ANNUAL, SEMIANNUAL, QUARTERLY, MONTHLY
    /// - DayCount: ACTUAL360, ACTUAL365, THIRTY360
    /// - RollConvention: NOROLL, ENDOFMONTH
    /// </remarks>
    [HttpPost("value")]
    [ProducesResponseType(typeof(BondValuationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public ActionResult<BondValuationResponseDto> Value([FromBody] BondValuationRequestDto request)
    {
            var currency = MapCurrency(request.Currency);
            var frequency = MapFrequency(request.Frequency);
            var dayCount = MapDayCount(request.DayCount);
            var rollConvention = MapRollConvention(request.RollConvention);

            var notional = NotionalModule.create(request.Notional, currency);

            var issueDate = ToLocalDate(request.IssueDate);
            var maturityDate = ToLocalDate(request.MaturityDate);
            var valuationDate = ToLocalDate(request.ValuationDate);

            var bond =
                BondModule.create(
                    notional,
                    request.CouponRate,
                    issueDate,
                    maturityDate,
                    frequency,
                    dayCount);

            var projection = BondProjection.projector(rollConvention, bond);

            var curve = YieldCurve.NewFlat(request.CurveRate);

            var result =
                Valuation.valueProjected(
                    valuationDate,
                    dayCount,
                    YieldCurve.NewFlat(request.CurveRate),
                    projection);

            var response = _mapper.Map<BondValuationResponseDto>(result);

            return Ok(response);
    }

    /// <summary>
    /// Converts a <see cref="DateOnly"/> value to a NodaTime <see cref="LocalDate"/>.
    /// </summary>
    /// <param name="value">The DateOnly value to convert.</param>
    /// <returns>A NodaTime LocalDate representation.</returns>
    private static LocalDate ToLocalDate(DateOnly value) =>
        new(value.Year, value.Month, value.Day);

    /// <summary>
    /// Maps a string currency code to a <see cref="Currency"/> domain object.
    /// </summary>
    /// <param name="value">The currency code (e.g., "USD", "EUR", "GBP").</param>
    /// <returns>The corresponding Currency domain object.</returns>
    /// <exception cref="ArgumentException">Thrown when the currency code is not supported.</exception>
    private static Currency MapCurrency(string value) =>
        value.Trim().ToUpperInvariant() switch
        {
            "USD" => Currency.USD,
            "EUR" => Currency.EUR,
            "GBP" => Currency.GBP,
            _ => throw new ArgumentException($"Unsupported currency '{value}'.")
        };

    /// <summary>
    /// Maps a string frequency value to a <see cref="Frequency"/> domain object.
    /// </summary>
    /// <param name="value">The frequency value (e.g., "ANNUAL", "SEMIANNUAL", "QUARTERLY", "MONTHLY").</param>
    /// <returns>The corresponding Frequency domain object.</returns>
    /// <exception cref="ArgumentException">Thrown when the frequency value is not supported.</exception>
    private static Frequency MapFrequency(string value) =>
        value.Trim().ToUpperInvariant() switch
        {
            "ANNUAL" => Frequency.Annual,
            "SEMIANNUAL" => Frequency.SemiAnnual,
            "QUARTERLY" => Frequency.Quarterly,
            "MONTHLY" => Frequency.Monthly,
            _ => throw new ArgumentException($"Unsupported frequency '{value}'.")
        };

    /// <summary>
    /// Maps a string day count convention to a <see cref="DayCountConvention"/> domain object.
    /// </summary>
    /// <param name="value">The day count convention (e.g., "ACTUAL360", "ACTUAL365", "THIRTY360").</param>
    /// <returns>The corresponding DayCountConvention domain object.</returns>
    /// <exception cref="ArgumentException">Thrown when the day count convention is not supported.</exception>
    private static DayCountConvention MapDayCount(string value) =>
        value.Trim().ToUpperInvariant() switch
        {
            "ACTUAL360" => DayCountConvention.Actual360,
            "ACTUAL365" => DayCountConvention.Actual365,
            "THIRTY360" => DayCountConvention.Thirty360,
            _ => throw new ArgumentException($"Unsupported day count '{value}'.")
        };

    /// <summary>
    /// Maps a string roll convention to a <see cref="RollConvention"/> domain object.
    /// </summary>
    /// <param name="value">The roll convention (e.g., "NOROLL", "ENDOFMONTH").</param>
    /// <returns>The corresponding RollConvention domain object.</returns>
    /// <exception cref="ArgumentException">Thrown when the roll convention is not supported.</exception>
    private static RollConvention MapRollConvention(string value) =>
        value.Trim().ToUpperInvariant() switch
        {
            "NOROLL" => RollConvention.NoRoll,
            "ENDOFMONTH" => RollConvention.EndOfMonth,
            _ => throw new ArgumentException($"Unsupported roll convention '{value}'.")
        };
}