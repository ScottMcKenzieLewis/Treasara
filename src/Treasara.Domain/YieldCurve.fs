namespace Treasara.Domain

open NodaTime

type YieldCurve =
    | Flat of decimal  // e.g. 0.045m for 4.5%

module YieldCurve =

    /// Get a continuously-compounded discount factor.
    /// DF = exp(-r * t)
    let discountFactor
        (dayCount: DayCountConvention)
        (valuationDate: ValuationDate)
        (paymentDate: LocalDate)
        (curve: YieldCurve)
        : decimal =

        if paymentDate < valuationDate then
            invalidArg "paymentDate" "Payment date cannot be before valuation date."

        let t = DayCount.yearFraction dayCount valuationDate paymentDate

        match curve with
        | Flat r ->
            // Use float exp then convert back to decimal.
            // (Good enough for v1; we can improve later.)
            let df = System.Math.Exp(- (float r) * (float t))
            decimal df