using AutoMapper;
using NodaTime;
using Treasara.Api.Dtos.Requests;
using Treasara.Domain;

namespace Treasara.Api.Mapping.Requests;

/// <summary>
/// Mapper for converting bond valuation request DTOs to domain objects.
/// </summary>
/// <remarks>
/// This mapper handles the translation of API request DTOs containing string-based
/// enumeration values (currency codes, frequency names, etc.) into strongly-typed
/// domain objects. It performs validation by throwing ArgumentException for unsupported
/// values, which are then caught by the global exception handler.
/// </remarks>
public sealed class BondRequestMapper : IBondRequestMapper
{
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="BondRequestMapper"/> class.
    /// </summary>
    /// <param name="mapper">The AutoMapper instance for mapping primitive types (e.g., DateOnly to LocalDate).</param>
    public BondRequestMapper(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// Maps a bond valuation request DTO to a validated Bond domain object.
    /// </summary>
    /// <param name="request">The bond valuation request DTO containing all bond parameters.</param>
    /// <returns>
    /// A fully validated Bond domain object ready for valuation calculations.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the request contains unsupported values for currency, frequency, or day count convention.
    /// </exception>
    /// <exception cref="DomainValidationException">
    /// Thrown by the domain layer when the bond parameters violate business rules
    /// (e.g., negative notional, maturity before issue date, negative coupon rate).
    /// </exception>
    /// <remarks>
    /// This method performs the following transformations:
    /// <list type="number">
    /// <item><description>Converts string currency code to Currency domain type</description></item>
    /// <item><description>Converts string frequency to Frequency domain type</description></item>
    /// <item><description>Converts string day count convention to DayCountConvention domain type</description></item>
    /// <item><description>Maps DateOnly values to NodaTime LocalDate</description></item>
    /// <item><description>Creates a Notional with validation</description></item>
    /// <item><description>Creates and validates the Bond domain object</description></item>
    /// </list>
    /// The returned Bond has already passed all domain validation rules.
    /// </remarks>
    public Bond MapToBond(BondValuationRequestDto request)
    {
        var currency = MapCurrency(request.Currency);
        var frequency = MapFrequency(request.Frequency);
        var dayCount = MapDayCount(request.DayCount);

        var issueDate = _mapper.Map<LocalDate>(request.IssueDate);
        var maturityDate = _mapper.Map<LocalDate>(request.MaturityDate);

        var notional = NotionalModule.create(request.Notional, currency);

        return BondModule.create(
            notional,
            request.CouponRate,
            issueDate,
            maturityDate,
            frequency,
            dayCount);
    }

    /// <summary>
    /// Maps a string roll convention value to a RollConvention domain type.
    /// </summary>
    /// <param name="value">The roll convention string (case-insensitive).</param>
    /// <returns>The corresponding RollConvention domain value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is not a recognized roll convention.
    /// Supported values: "NOROLL", "ENDOFMONTH".
    /// </exception>
    /// <remarks>
    /// The mapping is case-insensitive and trims whitespace. This method is public
    /// to allow the controller to map roll convention independently when needed.
    /// </remarks>
    public RollConvention MapRollConvention(string value) =>
        value.Trim().ToUpperInvariant() switch
        {
            "NOROLL" => RollConvention.NoRoll,
            "ENDOFMONTH" => RollConvention.EndOfMonth,
            _ => throw new ArgumentException($"Unsupported roll convention '{value}'.")
        };

    /// <summary>
    /// Maps a string currency code to a Currency domain type.
    /// </summary>
    /// <param name="value">The three-letter currency code (case-insensitive).</param>
    /// <returns>The corresponding Currency domain value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is not a recognized currency code.
    /// Supported values: "USD", "EUR", "GBP".
    /// </exception>
    /// <remarks>
    /// The mapping is case-insensitive and trims whitespace before validation.
    /// </remarks>
    private static Currency MapCurrency(string value) =>
        value.Trim().ToUpperInvariant() switch
        {
            "USD" => Currency.USD,
            "EUR" => Currency.EUR,
            "GBP" => Currency.GBP,
            _ => throw new ArgumentException($"Unsupported currency '{value}'.")
        };

    /// <summary>
    /// Maps a string frequency value to a Frequency domain type.
    /// </summary>
    /// <param name="value">The payment frequency string (case-insensitive).</param>
    /// <returns>The corresponding Frequency domain value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is not a recognized payment frequency.
    /// Supported values: "ANNUAL", "SEMIANNUAL", "QUARTERLY", "MONTHLY".
    /// </exception>
    /// <remarks>
    /// The mapping is case-insensitive and trims whitespace before validation.
    /// These frequencies determine the bond's payment schedule generation.
    /// </remarks>
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
    /// Maps a string day count convention to a DayCountConvention domain type.
    /// </summary>
    /// <param name="value">The day count convention string (case-insensitive).</param>
    /// <returns>The corresponding DayCountConvention domain value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is not a recognized day count convention.
    /// Supported values: "ACTUAL360", "ACTUAL365", "THIRTY360".
    /// </exception>
    /// <remarks>
    /// The mapping is case-insensitive and trims whitespace before validation.
    /// Day count conventions determine how interest accrues over time periods.
    /// </remarks>
    private static DayCountConvention MapDayCount(string value) =>
        value.Trim().ToUpperInvariant() switch
        {
            "ACTUAL360" => DayCountConvention.Actual360,
            "ACTUAL365" => DayCountConvention.Actual365,
            "THIRTY360" => DayCountConvention.Thirty360,
            _ => throw new ArgumentException($"Unsupported day count '{value}'.")
        };
}