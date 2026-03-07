namespace Treasara.Domain

/// <summary>
/// Represents the face value or principal amount of a financial instrument.
/// </summary>
/// <remarks>
/// The notional amount is a fundamental concept in fixed-income securities, representing
/// the principal value upon which interest calculations are based and which is typically
/// repaid at maturity. This value object ensures that notional amounts are always positive
/// and associated with a specific currency.
/// </remarks>
type Notional =
    { /// <summary>The principal amount, which must be positive.</summary>
      Amount: decimal
      /// <summary>The currency in which the notional is denominated.</summary>
      Currency: Currency }

/// <summary>
/// Module containing operations for creating and validating Notional values.
/// </summary>
module Notional =

    /// <summary>
    /// Creates a new Notional instance with the specified amount and currency.
    /// </summary>
    /// <param name="amount">The principal amount, which must be positive.</param>
    /// <param name="currency">The currency of the notional amount.</param>
    /// <returns>A new validated Notional instance.</returns>
    /// <exception cref="DomainValidationException">
    /// Thrown when the amount is zero or negative. Notional amounts must represent
    /// positive principal values in financial instruments.
    /// </exception>
    /// <remarks>
    /// This function enforces the business rule that notional amounts must be strictly
    /// positive, as negative or zero principal values are not meaningful in the context
    /// of bond valuation and cash flow calculations.
    /// </remarks>
    let create amount currency =
        if amount <= 0m then
            raise (DomainValidationException("Notional must be positive."))

        { Amount = amount
          Currency = currency }