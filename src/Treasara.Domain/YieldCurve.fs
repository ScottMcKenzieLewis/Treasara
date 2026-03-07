namespace Treasara.Domain

open NodaTime

/// <summary>
/// Represents a yield curve used for discounting future cash flows to present value.
/// </summary>
/// <remarks>
/// A yield curve models the relationship between time to maturity and interest rates,
/// which is essential for valuing fixed-income instruments. In version 1, only a flat
/// yield curve is supported, where the same rate applies to all maturities. Future
/// versions may include interpolated curves with multiple tenor points.
/// </remarks>
type YieldCurve =
    /// <summary>
    /// A flat yield curve where a single rate applies to all maturities.
    /// </summary>
    /// <remarks>
    /// The rate is expressed as a decimal (e.g., 0.045 represents 4.5% annual rate).
    /// This simplified curve structure is suitable for basic valuation scenarios
    /// and serves as a baseline implementation.
    /// </remarks>
    | Flat of decimal

/// <summary>
/// Module containing functions for working with yield curves and calculating discount factors.
/// </summary>
module YieldCurve =

    /// <summary>
    /// Calculates a continuously-compounded discount factor for a future payment date.
    /// </summary>
    /// <param name="dayCount">The day count convention for calculating the time fraction.</param>
    /// <param name="valuationDate">The present date from which discounting is performed.</param>
    /// <param name="paymentDate">The future date when the cash flow occurs.</param>
    /// <param name="curve">The yield curve providing the discount rate.</param>
    /// <returns>
    /// The discount factor as a decimal value between 0 and 1, where values closer to 1
    /// represent near-term payments and values closer to 0 represent distant payments.
    /// </returns>
    /// <exception cref="DomainValidationException">
    /// Thrown when the payment date is before the valuation date, as discounting
    /// past cash flows is not meaningful in standard valuation contexts.
    /// </exception>
    /// <remarks>
    /// The discount factor is calculated using continuous compounding:
    /// <para>DF = exp(-r × t)</para>
    /// where:
    /// <list type="bullet">
    /// <item><description>r is the continuously-compounded interest rate from the yield curve</description></item>
    /// <item><description>t is the time in years from valuation date to payment date</description></item>
    /// <item><description>exp is the exponential function (e^x)</description></item>
    /// </list>
    /// <para>
    /// Implementation note: The calculation uses floating-point arithmetic (via System.Math.Exp)
    /// and converts back to decimal. This approach is sufficient for version 1 accuracy requirements
    /// but may be refined in future versions for improved numerical precision.
    /// </para>
    /// <para>
    /// The discount factor represents the present value of $1 received at the payment date.
    /// For example, a discount factor of 0.95 means that $1 in the future is worth $0.95 today.
    /// </para>
    /// </remarks>
    let discountFactor
        (dayCount: DayCountConvention)
        (valuationDate: ValuationDate)
        (paymentDate: LocalDate)
        (curve: YieldCurve)
        : decimal =

        if paymentDate < valuationDate then
            raise (DomainValidationException("Payment date cannot be before valuation date."))

        let t = DayCount.yearFraction dayCount valuationDate paymentDate

        match curve with
        | Flat r ->
            // Use float exp then convert back to decimal.
            // (Good enough for v1; we can improve later.)
            let df = System.Math.Exp(-(float r) * (float t))
            decimal df