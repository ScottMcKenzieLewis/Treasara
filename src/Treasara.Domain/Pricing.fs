namespace Treasara.Domain

module Pricing =

    let pvCashflow
        (valuationDate: ValuationDate)
        (dayCount: DayCountConvention)
        (curve: YieldCurve)
        (cf: Cashflow)
        : Money =

        let df =
            YieldCurve.discountFactor dayCount valuationDate cf.PaymentDate curve

        { cf.Amount with Amount = cf.Amount.Amount * df }

    let pvCashflows
        (valuationDate: ValuationDate)
        (dayCount: DayCountConvention)
        (curve: YieldCurve)
        (cfs: Cashflow list)
        : Money option =

        cfs
        |> List.map (pvCashflow valuationDate dayCount curve)
        |> Money.sumSameCurrency

    let pvBond
        (valuationDate: ValuationDate)
        (curveDayCount: DayCountConvention)
        (curve: YieldCurve)
        (roll: RollConvention)
        (bond: Bond)
        : Money option =

        let cashflows =
            BondCashflows.generate roll bond

        pvCashflows valuationDate curveDayCount curve cashflows