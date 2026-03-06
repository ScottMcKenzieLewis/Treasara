namespace Treasara.Domain

open NodaTime

type ValuationLine =
    { PaymentDate : LocalDate
      CashflowAmount : Money
      DiscountFactor : decimal
      PresentValue : Money }

type ValuationResult<'a> =
    { Target : 'a
      Lines : ValuationLine list
      TotalPresentValue : Money option }

module Valuation =

    let private valueCashflow
        (valuationDate: ValuationDate)
        (dayCount: DayCountConvention)
        (curve: YieldCurve)
        (cf: Cashflow)
        : ValuationLine =

        let df =
            YieldCurve.discountFactor dayCount valuationDate cf.PaymentDate curve

        let pvAmount =
            cf.Amount.Amount * df

        { PaymentDate = cf.PaymentDate
          CashflowAmount = cf.Amount
          DiscountFactor = df
          PresentValue =
              { Amount = pvAmount
                Currency = cf.Amount.Currency } }

    let valueProjected
        (valuationDate: ValuationDate)
        (dayCount: DayCountConvention)
        (curve: YieldCurve)
        (projection: ProjectedCashflows<'a>)
        : ValuationResult<'a> =

        let lines =
            projection.Cashflows
            |> List.map (valueCashflow valuationDate dayCount curve)

        let total =
            lines
            |> List.map (fun line -> line.PresentValue)
            |> Money.sumSameCurrency

        { Target = projection.Target
          Lines = lines
          TotalPresentValue = total }

    let valueWithProjector
        (valuationDate: ValuationDate)
        (dayCount: DayCountConvention)
        (curve: YieldCurve)
        (projector: CashflowProjector<'a>)
        (target: 'a)
        : ValuationResult<'a> =

        target
        |> projector
        |> valueProjected valuationDate dayCount curve