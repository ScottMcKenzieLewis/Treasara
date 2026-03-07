namespace Treasara.Domain

open NodaTime

/// <summary>
/// Represents floating rate indices used in interest rate swap contracts.
/// </summary>
/// <remarks>
/// Floating rate indices are reference rates that determine the interest payments
/// on the floating leg of a swap. These rates are typically observed at the start
/// of each accrual period and applied to calculate the period's interest payment.
/// </remarks>
type FloatIndex =
    /// <summary>Secured Overnight Financing Rate, a U.S. dollar reference rate.</summary>
    | SOFR
    /// <summary>London Interbank Offered Rate for 3-month USD (legacy rate being phased out).</summary>
    | LIBOR3M
    /// <summary>Euro Interbank Offered Rate for 6-month EUR.</summary>
    | EURIBOR6M

/// <summary>
/// Distinguishes between fixed and floating legs in an interest rate swap.
/// </summary>
/// <remarks>
/// In an interest rate swap, one party typically pays a fixed rate while receiving
/// a floating rate, or vice versa. This type identifies which side of the exchange
/// a particular leg represents.
/// </remarks>
type SwapLegType =
    /// <summary>A leg with fixed periodic payments based on a predetermined rate.</summary>
    | Fixed
    /// <summary>A leg with variable payments determined by a floating rate index.</summary>
    | Floating

/// <summary>
/// Represents one side (leg) of an interest rate swap contract.
/// </summary>
/// <remarks>
/// A swap leg defines the payment characteristics for one party in the swap agreement,
/// including the notional amount, payment frequency, and whether payments are based
/// on a fixed rate or a floating rate index.
/// </remarks>
type SwapLeg =
    { /// <summary>Indicates whether this is a fixed or floating leg.</summary>
      LegType: SwapLegType
      /// <summary>The notional principal amount used to calculate interest payments.</summary>
      Notional: Notional
      /// <summary>The fixed interest rate (required for Fixed legs, must be None for Floating legs).</summary>
      FixedRate: decimal option
      /// <summary>The floating rate index (required for Floating legs, must be None for Fixed legs).</summary>
      FloatIndex: FloatIndex option
      /// <summary>The frequency of interest payments.</summary>
      Frequency: Frequency
      /// <summary>The day count convention for calculating interest accruals.</summary>
      DayCount: DayCountConvention }

/// <summary>
/// Represents an interest rate swap agreement between two parties.
/// </summary>
/// <remarks>
/// An interest rate swap is a derivative contract where two parties exchange interest
/// payment obligations based on a notional principal amount. Typically, one party pays
/// a fixed rate while receiving a floating rate, allowing both parties to manage their
/// interest rate exposure. The notional principal is not exchanged; it is only used to
/// calculate the interest payments.
/// </remarks>
type InterestRateSwap =
    { /// <summary>The effective date when the swap begins and interest starts accruing.</summary>
      StartDate: LocalDate
      /// <summary>The termination date when the swap ends.</summary>
      MaturityDate: LocalDate
      /// <summary>The leg representing payments made by the perspective party.</summary>
      PayLeg: SwapLeg
      /// <summary>The leg representing payments received by the perspective party.</summary>
      ReceiveLeg: SwapLeg }

/// <summary>
/// Module containing functions for creating and validating interest rate swaps and swap legs.
/// </summary>
module Swap =

    /// <summary>
    /// Validates a swap leg's business rules and consistency requirements.
    /// </summary>
    /// <param name="leg">The swap leg to validate.</param>
    /// <returns>The validated swap leg if all rules pass.</returns>
    /// <exception cref="DomainValidationException">
    /// Thrown when validation fails due to:
    /// <list type="bullet">
    /// <item><description>Notional amount is not positive</description></item>
    /// <item><description>Fixed leg missing a fixed rate</description></item>
    /// <item><description>Fixed leg has a negative rate</description></item>
    /// <item><description>Fixed leg incorrectly has a floating index</description></item>
    /// <item><description>Floating leg incorrectly has a fixed rate</description></item>
    /// <item><description>Floating leg missing a floating index</description></item>
    /// </list>
    /// </exception>
    let private validateLeg (leg: SwapLeg) =
        if leg.Notional.Amount <= 0m then
            raise (DomainValidationException("Swap leg notional must be positive."))

        match leg.LegType, leg.FixedRate, leg.FloatIndex with
        | Fixed, None, _ ->
            raise (DomainValidationException("Fixed leg must have a fixed rate."))
        | Fixed, Some rate, _ when rate < 0m ->
            raise (DomainValidationException("Fixed leg rate cannot be negative."))
        | Fixed, Some _, Some _ ->
            raise (DomainValidationException("Fixed leg cannot also have a floating index."))
        | Floating, Some _, _ ->
            raise (DomainValidationException("Floating leg cannot have a fixed rate."))
        | Floating, _, None ->
            raise (DomainValidationException("Floating leg must have a floating index."))
        | _ -> leg

    /// <summary>
    /// Validates an interest rate swap's business rules and structural consistency.
    /// </summary>
    /// <param name="swap">The interest rate swap to validate.</param>
    /// <returns>The validated swap if all rules pass.</returns>
    /// <exception cref="DomainValidationException">
    /// Thrown when validation fails due to:
    /// <list type="bullet">
    /// <item><description>Maturity date is not after start date</description></item>
    /// <item><description>Either leg fails its individual validation</description></item>
    /// <item><description>Pay and receive legs have different currencies</description></item>
    /// <item><description>Pay and receive legs have different notional amounts (v1 constraint)</description></item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// In version 1 of this system, cross-currency swaps and amortizing/accreting swaps
    /// (with different notionals) are not supported. Both legs must have the same currency
    /// and notional amount.
    /// </remarks>
    let private validate (swap: InterestRateSwap) =
        if swap.MaturityDate <= swap.StartDate then
            raise (DomainValidationException("MaturityDate must be after StartDate."))

        validateLeg swap.PayLeg |> ignore
        validateLeg swap.ReceiveLeg |> ignore

        if swap.PayLeg.Notional.Currency <> swap.ReceiveLeg.Notional.Currency then
            raise (DomainValidationException("Pay and receive legs must have the same currency."))

        if swap.PayLeg.Notional.Amount <> swap.ReceiveLeg.Notional.Amount then
            raise (
                DomainValidationException("Pay and receive legs must have the same notional in v1.")
            )

        swap

    /// <summary>
    /// Creates a validated fixed-rate swap leg.
    /// </summary>
    /// <param name="notional">The notional principal amount.</param>
    /// <param name="fixedRate">The fixed interest rate as a decimal (e.g., 0.03 for 3%).</param>
    /// <param name="frequency">The payment frequency for this leg.</param>
    /// <param name="dayCount">The day count convention for interest calculations.</param>
    /// <returns>A validated fixed swap leg.</returns>
    /// <exception cref="DomainValidationException">
    /// Thrown if validation fails (e.g., negative rate, non-positive notional).
    /// </exception>
    /// <remarks>
    /// Fixed legs pay a predetermined rate on each payment date, calculated using
    /// the specified notional, rate, frequency, and day count convention.
    /// </remarks>
    let createFixedLeg
        (notional: Notional)
        (fixedRate: decimal)
        (frequency: Frequency)
        (dayCount: DayCountConvention)
        : SwapLeg =
        { LegType = Fixed
          Notional = notional
          FixedRate = Some fixedRate
          FloatIndex = None
          Frequency = frequency
          DayCount = dayCount }
        |> validateLeg

    /// <summary>
    /// Creates a validated floating-rate swap leg.
    /// </summary>
    /// <param name="notional">The notional principal amount.</param>
    /// <param name="floatIndex">The floating rate index to reference (e.g., SOFR, LIBOR3M).</param>
    /// <param name="frequency">The payment frequency for this leg.</param>
    /// <param name="dayCount">The day count convention for interest calculations.</param>
    /// <returns>A validated floating swap leg.</returns>
    /// <exception cref="DomainValidationException">
    /// Thrown if validation fails (e.g., non-positive notional).
    /// </exception>
    /// <remarks>
    /// Floating legs make variable payments based on the observed rate of the specified
    /// floating index at the start of each accrual period. The actual rate used would be
    /// determined by market observations at fixing dates.
    /// </remarks>
    let createFloatingLeg
        (notional: Notional)
        (floatIndex: FloatIndex)
        (frequency: Frequency)
        (dayCount: DayCountConvention)
        : SwapLeg =
        { LegType = Floating
          Notional = notional
          FixedRate = None
          FloatIndex = Some floatIndex
          Frequency = frequency
          DayCount = dayCount }
        |> validateLeg

    /// <summary>
    /// Creates a validated interest rate swap from two swap legs.
    /// </summary>
    /// <param name="startDate">The effective date when the swap begins.</param>
    /// <param name="maturityDate">The termination date when the swap ends.</param>
    /// <param name="payLeg">The leg representing payments made.</param>
    /// <param name="receiveLeg">The leg representing payments received.</param>
    /// <returns>A fully validated interest rate swap.</returns>
    /// <exception cref="DomainValidationException">
    /// Thrown if validation fails for any swap-level or leg-level rules.
    /// </exception>
    /// <remarks>
    /// The swap is validated to ensure:
    /// <list type="bullet">
    /// <item><description>Valid date range (maturity after start)</description></item>
    /// <item><description>Both legs are individually valid</description></item>
    /// <item><description>Currency and notional consistency between legs</description></item>
    /// </list>
    /// A typical interest rate swap has one party paying fixed and receiving floating,
    /// allowing them to convert floating-rate exposure to fixed-rate, or vice versa.
    /// </remarks>
    let create
        (startDate: LocalDate)
        (maturityDate: LocalDate)
        (payLeg: SwapLeg)
        (receiveLeg: SwapLeg)
        : InterestRateSwap =
        { StartDate = startDate
          MaturityDate = maturityDate
          PayLeg = payLeg
          ReceiveLeg = receiveLeg }
        |> validate