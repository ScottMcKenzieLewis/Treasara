using NodaTime;
using Treasara.Api.Dtos.Requests;
using Treasara.Domain;

namespace Treasara.Api.Mapping.Requests;

/// <summary>
/// Defines the contract for mapping bond valuation request DTOs to domain objects.
/// </summary>
/// <remarks>
/// This interface abstracts the mapping logic for converting API request DTOs
/// containing string-based enumeration values into strongly-typed domain objects.
/// Implementations should validate input values and throw appropriate exceptions
/// for unsupported or invalid data.
/// </remarks>
public interface IBondRequestMapper
{
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
    /// Thrown when the bond parameters violate business rules
    /// (e.g., negative notional, maturity before issue date, negative coupon rate).
    /// </exception>
    Bond MapToBond(BondValuationRequestDto request);

    /// <summary>
    /// Maps a string roll convention value to a RollConvention domain type.
    /// </summary>
    /// <param name="value">The roll convention string (case-insensitive).</param>
    /// <returns>The corresponding RollConvention domain value.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is not a recognized roll convention.
    /// Supported values: "NOROLL", "ENDOFMONTH".
    /// </exception>
    RollConvention MapRollConvention(string value);
}