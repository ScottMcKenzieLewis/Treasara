using Asp.Versioning;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Treasara.Api.Dtos;
using Treasara.Api.Dtos.Requests;
using Treasara.Api.Dtos.Responses;
using Treasara.Api.Mapping.Requests;
using Treasara.Domain;

namespace Treasara.Api.Controllers;

/// <summary>
/// API controller for bond valuation operations.
/// </summary>
/// <remarks>
/// Provides endpoints for calculating bond valuations, including present value,
/// accrued interest, and cash flow projections based on market yield curves.
/// This controller uses FluentValidation for request validation and custom mappers
/// to translate API DTOs into domain objects.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public sealed class BondsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IValidator<BondValuationRequestDto> _validator;
    private readonly IBondRequestMapper _bondRequestMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="BondsController"/> class.
    /// </summary>
    /// <param name="mapper">The AutoMapper instance for DTO mappings.</param>
    /// <param name="validator">The FluentValidation validator for bond valuation requests.</param>
    /// <param name="bondRequestMapper">The custom mapper for converting request DTOs to domain objects.</param>
    public BondsController(
        IMapper mapper,
        IValidator<BondValuationRequestDto> validator,
        IBondRequestMapper bondRequestMapper)
    {
        _mapper = mapper;
        _validator = validator;
        _bondRequestMapper = bondRequestMapper;
    }

    /// <summary>
    /// Calculates the valuation of a bond based on the provided parameters.
    /// </summary>
    /// <param name="request">The bond valuation request containing bond characteristics and market data.</param>
    /// <returns>A detailed bond valuation including present value, accrued interest, and cash flows.</returns>
    /// <response code="200">Returns the calculated bond valuation with detailed cash flow breakdown.</response>
    /// <response code="400">
    /// If the request contains invalid parameters. Returns validation errors with field-level details
    /// including unsupported values for currency, frequency, day count convention, or roll convention.
    /// </response>
    /// <response code="500">If an unexpected server error occurs during valuation processing.</response>
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
    ///         "valuationDate": "2026-03-07",
    ///         "frequency": "SEMIANNUAL",
    ///         "dayCount": "THIRTY360",
    ///         "rollConvention": "NOROLL",
    ///         "curveRate": 0.045
    ///     }
    /// 
    /// Sample response:
    /// 
    ///     {
    ///         "instrumentType": "Bond",
    ///         "totalPresentValue": 1035420.75,
    ///         "currency": "USD",
    ///         "lines": [
    ///             {
    ///                 "paymentDate": "2026-07-01",
    ///                 "cashflowAmount": 25000.00,
    ///                 "discountFactor": 0.9825,
    ///                 "presentValue": 24562.50,
    ///                 "currency": "USD"
    ///             }
    ///         ]
    ///     }
    /// 
    /// Validation rules:
    /// <list type="bullet">
    /// <item><description>Notional must be positive</description></item>
    /// <item><description>CouponRate must be non-negative</description></item>
    /// <item><description>MaturityDate must be after IssueDate</description></item>
    /// <item><description>ValuationDate must not be before IssueDate</description></item>
    /// <item><description>All string fields (Currency, Frequency, DayCount, RollConvention) must not be empty</description></item>
    /// </list>
    /// 
    /// Supported values:
    /// <list type="table">
    /// <listheader>
    /// <term>Parameter</term>
    /// <description>Valid Values</description>
    /// </listheader>
    /// <item>
    /// <term>Currency</term>
    /// <description>USD, EUR, GBP (case-insensitive)</description>
    /// </item>
    /// <item>
    /// <term>Frequency</term>
    /// <description>ANNUAL, SEMIANNUAL, QUARTERLY, MONTHLY (case-insensitive)</description>
    /// </item>
    /// <item>
    /// <term>DayCount</term>
    /// <description>ACTUAL360, ACTUAL365, THIRTY360 (case-insensitive)</description>
    /// </item>
    /// <item>
    /// <term>RollConvention</term>
    /// <description>NOROLL, ENDOFMONTH (case-insensitive)</description>
    /// </item>
    /// </list>
    /// 
    /// The valuation process:
    /// <list type="number">
    /// <item><description>Validates the request using FluentValidation rules</description></item>
    /// <item><description>Maps the request DTO to domain objects (Bond, RollConvention, etc.)</description></item>
    /// <item><description>Generates a cash flow projection using the bond's payment schedule</description></item>
    /// <item><description>Discounts each cash flow to present value using the flat yield curve</description></item>
    /// <item><description>Returns the detailed valuation result with line-by-line breakdown</description></item>
    /// </list>
    /// </remarks>
    [HttpPost("value")]
    [ProducesResponseType(typeof(BondValuationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorDto), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BondValuationResponseDto>> Value([FromBody] BondValuationRequestDto request)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new
            {
                error = "validation_error",
                message = "Request validation failed.",
                details = errors,
                traceId = HttpContext.TraceIdentifier
            });
        }

        var bond = _bondRequestMapper.MapToBond(request);
        var rollConvention = _bondRequestMapper.MapRollConvention(request.RollConvention);
        var valuationDate = _mapper.Map<LocalDate>(request.ValuationDate);

        var projection = BondProjection.projector(rollConvention, bond);

        var result =
            Valuation.valueProjected(
                valuationDate,
                bond.DayCount,
                YieldCurve.NewFlat(request.CurveRate),
                projection);

        var response = _mapper.Map<BondValuationResponseDto>(result);

        return Ok(response);
    }
}