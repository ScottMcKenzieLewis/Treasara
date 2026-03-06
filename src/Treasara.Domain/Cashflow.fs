namespace Treasara.Domain

open NodaTime

type Cashflow =
    { PaymentDate : LocalDate
      Amount : Money }

module Cashflow =

    let create (paymentDate: LocalDate) (amount: Money) : Cashflow =
        { PaymentDate = paymentDate
          Amount = amount }

    let sortByDate (cfs: Cashflow list) =
        cfs |> List.sortBy (fun cf -> cf.PaymentDate)

module CashflowMath =

    let sumSameCurrency (cfs: Cashflow list) : Money option =
        match cfs with
        | [] -> None
        | first :: _ ->
            let currency = first.Amount.Currency

            if cfs |> List.exists (fun cf -> cf.Amount.Currency <> currency) then
                invalidArg "cfs" "Cannot sum cashflows with mixed currencies."

            let total = cfs |> List.sumBy (fun cf -> cf.Amount.Amount)

            Some
                { Amount = total
                  Currency = currency }

module BondCashflows =

    let generate
        (roll: RollConvention)
        (bond: Bond)
        : Cashflow list =

        let periods =
            Schedule.generate bond.Frequency roll bond.IssueDate bond.MaturityDate

        let coupons =
            periods
            |> List.map (fun p ->
                let yf =
                    DayCount.yearFraction bond.DayCount p.AccrualStart p.AccrualEnd

                let couponAmount =
                    bond.Notional.Amount * bond.CouponRate * yf

                Cashflow.create
                    p.PaymentDate
                    { Amount = couponAmount
                      Currency = bond.Notional.Currency })

        let principal =
            Cashflow.create
                bond.MaturityDate
                { Amount = bond.Notional.Amount
                  Currency = bond.Notional.Currency }

        (coupons @ [ principal ])
        |> Cashflow.sortByDate