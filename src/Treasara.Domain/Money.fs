namespace Treasara.Domain

/// <summary>
/// Represents a monetary amount with an associated currency.
/// </summary>
/// <remarks>
/// Money is a value object that encapsulates both a numeric amount and its currency,
/// ensuring that monetary operations maintain currency consistency and preventing
/// accidental mixing of different currencies in calculations.
/// </remarks>
type Money =
    { /// <summary>The numeric value of the monetary amount.</summary>
      Amount: decimal
      /// <summary>The currency in which the amount is denominated.</summary>
      Currency: Currency }

/// <summary>
/// Module containing operations for creating and manipulating Money values.
/// </summary>
module Money =

    /// <summary>
    /// Creates a new Money instance with the specified amount and currency.
    /// </summary>
    /// <param name="amount">The numeric amount.</param>
    /// <param name="currency">The currency of the amount.</param>
    /// <returns>A new Money instance.</returns>
    let create amount currency =
        { Amount = amount
          Currency = currency }

    /// <summary>
    /// Adds two Money values together.
    /// </summary>
    /// <param name="m1">The first money value.</param>
    /// <param name="m2">The second money value.</param>
    /// <returns>A new Money instance containing the sum of the two amounts in the same currency.</returns>
    /// <exception cref="DomainValidationException">
    /// Thrown when attempting to add money values with different currencies.
    /// Currency conversion must be performed explicitly before addition.
    /// </exception>
    /// <remarks>
    /// This function enforces currency consistency by validating that both operands
    /// have the same currency before performing the addition operation.
    /// </remarks>
    let add m1 m2 =
        if m1.Currency <> m2.Currency then
            raise (DomainValidationException("Cannot add money values with different currencies."))

        { m1 with Amount = m1.Amount + m2.Amount }

    /// <summary>
    /// Multiplies a Money value by a scalar factor.
    /// </summary>
    /// <param name="factor">The multiplier to apply to the amount.</param>
    /// <param name="money">The money value to scale.</param>
    /// <returns>A new Money instance with the scaled amount in the same currency.</returns>
    /// <remarks>
    /// This function is useful for operations such as calculating interest payments,
    /// discounting future values, or adjusting notional amounts. The currency remains
    /// unchanged while only the amount is multiplied.
    /// </remarks>
    let scale factor money =
        { money with Amount = money.Amount * factor }

    /// <summary>
    /// Calculates the sum of a list of Money values with the same currency.
    /// </summary>
    /// <param name="ms">The list of money values to sum.</param>
    /// <returns>
    /// Some Money containing the total if the list is non-empty; otherwise, None for an empty list.
    /// </returns>
    /// <exception cref="DomainValidationException">
    /// Thrown when the list contains money values with different currencies.
    /// </exception>
    /// <remarks>
    /// This function uses the add function internally, which ensures that all money values
    /// in the list share the same currency. If the list contains mixed currencies, an
    /// exception is raised during the fold operation.
    /// </remarks>
    let sumSameCurrency (ms: Money list) : Money option =
        match ms with
        | [] -> None
        | first :: rest ->
            let total = rest |> List.fold add first
            Some total