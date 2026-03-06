using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Treasara.Api.Dtos.Requests;
using Treasara.Api.Dtos.Responses;
using Treasara.Domain;

namespace Treasara.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class BondsController : ControllerBase
{
    private readonly IMapper _mapper;

    public BondsController(IMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpPost("value")]
    [ProducesResponseType(typeof(BondValuationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<BondValuationResponseDto> Value([FromBody] BondValuationRequestDto request)
    {
        try
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
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static LocalDate ToLocalDate(DateOnly value) =>
        new(value.Year, value.Month, value.Day);

    private static Currency MapCurrency(string value) =>
        value.Trim().ToUpperInvariant() switch
        {
            "USD" => Currency.USD,
            "EUR" => Currency.EUR,
            "GBP" => Currency.GBP,
            _ => throw new ArgumentException($"Unsupported currency '{value}'.")
        };

    private static Frequency MapFrequency(string value) =>
        value.Trim().ToUpperInvariant() switch
        {
            "ANNUAL" => Frequency.Annual,
            "SEMIANNUAL" => Frequency.SemiAnnual,
            "QUARTERLY" => Frequency.Quarterly,
            "MONTHLY" => Frequency.Monthly,
            _ => throw new ArgumentException($"Unsupported frequency '{value}'.")
        };

    private static DayCountConvention MapDayCount(string value) =>
        value.Trim().ToUpperInvariant() switch
        {
            "ACTUAL360" => DayCountConvention.Actual360,
            "ACTUAL365" => DayCountConvention.Actual365,
            "THIRTY360" => DayCountConvention.Thirty360,
            _ => throw new ArgumentException($"Unsupported day count '{value}'.")
        };

    private static RollConvention MapRollConvention(string value) =>
        value.Trim().ToUpperInvariant() switch
        {
            "NOROLL" => RollConvention.NoRoll,
            "ENDOFMONTH" => RollConvention.EndOfMonth,
            _ => throw new ArgumentException($"Unsupported roll convention '{value}'.")
        };
}