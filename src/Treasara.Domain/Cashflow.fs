namespace Treasara.Domain

open NodaTime

/// <summary>
/// Represents a future cash payment with a specified date and monetary amount.
/// </summary>
/// <remarks>
/// Cash flows are fundamental building blocks in fixed-income valuation, representing
/// individual payment obligations (such as coupon payments or principal repayments)
/// that occur at specific points in time.
/// </remarks>
type Cashflow =
    { /// <summary>The date on which the payment is scheduled to occur.</summary>
      PaymentDate: LocalDate
      /// <summary>The monetary amount of the payment, including currency.</summary>
      Amount: Money }

/// <summary>
/// Module containing basic operations for creating and manipulating cash flows.
/// </summary>
module Cashflow =

    /// <summary>
    /// Creates a new cash flow with the specified payment date and amount.
    /// </summary>
    /// <param name="paymentDate">The date the payment is scheduled.</param>
    /// <param name="amount">The monetary amount of the payment.</param>
    /// <returns>A new Cashflow instance.</returns>
    let create (paymentDate: LocalDate) (amount: Money) : Cashflow =
        { PaymentDate = paymentDate
          Amount = amount }

    /// <summary>
    /// Sorts a list of cash flows in chronological order by payment date.
    /// </summary>
    /// <param name="cfs">The list of cash flows to sort.</param>
    /// <returns>A new list of cash flows ordered by payment date (earliest first).</returns>
    /// <remarks>
    /// This function is useful for presenting cash flow schedules in temporal order
    /// and for sequential processing in valuation calculations.
    /// </remarks>
    let sortByDate (cfs: Cashflow list) =
        cfs |> List.sortBy (fun cf -> cf.PaymentDate)

/// <summary>
/// Module containing mathematical operations for cash flow aggregation and analysis.
/// </summary>
module CashflowMath =

    /// <summary>
    /// Calculates the total sum of cash flows with the same currency.
    /// </summary>
    /// <param name="cfs">The list of cash flows to sum.</param>
    /// <returns>
    /// Some Money representing the total if the list is non-empty and all cash flows
    /// share the same currency; otherwise, None for an empty list.
    /// </returns>
    /// <exception cref="DomainValidationException">
    /// Thrown when the cash flow list contains mixed currencies, as summing
    /// across different currencies is not permitted without exchange rates.
    /// </exception>
    /// <remarks>
    /// This function ensures currency consistency by validating that all cash flows
    /// use the same currency before performing the summation.
    /// </remarks>
    let sumSameCurrency (cfs: Cashflow list) : Money option =
        match cfs with
        | [] -> None
        | first :: _ ->
            let currency = first.Amount.Currency

            if cfs |> List.exists (fun cf -> cf.Amount.Currency <> currency) then
                raise (DomainValidationException("Cannot sum cashflows with mixed currencies."))

            let total = cfs |> List.sumBy (fun cf -> cf.Amount.Amount)

            Some
                { Amount = total
                  Currency = currency }

/// <summary>
/// Module containing specialized functions for generating bond-related cash flows.
/// </summary>
module BondCashflows =

    /// <summary>
    /// Generates the complete schedule of cash flows for a bond, including coupon payments and principal repayment.
    /// </summary>
    /// <param name="roll">The roll convention to apply when generating the payment schedule.</param>
    /// <param name="bond">The bond for which to generate cash flows.</param>
    /// <returns>
    /// A chronologically sorted list of cash flows, including all periodic coupon payments
    /// and the final principal repayment at maturity.
    /// </returns>
    /// <remarks>
    /// The function performs the following steps:
    /// <list type="number">
    /// <item><description>Generates the payment schedule based on the bond's frequency and roll convention</description></item>
    /// <item><description>Calculates coupon amounts for each period using the day count convention and coupon rate</description></item>
    /// <item><description>Adds the principal repayment at maturity</description></item>
    /// <item><description>Sorts all cash flows chronologically</description></item>
    /// </list>
    /// Each coupon payment is calculated as: Notional × CouponRate × YearFraction,
    /// where YearFraction is determined by the bond's day count convention.
    /// </remarks>
    let generate (roll: RollConvention) (bond: Bond) : Cashflow list =

        let periods =
            Schedule.generate bond.Frequency roll bond.IssueDate bond.MaturityDate

        let coupons =
            periods
            |> List.map (fun p ->
                let yf =
                    DayCount.yearFraction bond.DayCount p.AccrualStart p.AccrualEnd

                let couponAmount = bond.Notional.Amount * bond.CouponRate * yf

                Cashflow.create
                    p.PaymentDate
                    { Amount = couponAmount
                      Currency = bond.Notional.Currency })

        let principal =
            Cashflow.create
                bond.MaturityDate
                { Amount = bond.Notional.Amount
                  Currency = bond.Notional.Currency }

        (coupons @ [ principal ]) |> Cashflow.sortByDate