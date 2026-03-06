namespace Treasara.Domain

module SwapProjection =

    let private projectFixedLeg
        (roll: RollConvention)
        (startDate: NodaTime.LocalDate)
        (maturityDate: NodaTime.LocalDate)
        (leg: SwapLeg)
        : Cashflow list =

        let periods =
            Schedule.generate leg.Frequency roll startDate maturityDate

        periods
        |> List.map (fun p ->

            let yf =
                DayCount.yearFraction leg.DayCount p.AccrualStart p.AccrualEnd

            let amount =
                match leg.FixedRate with
                | Some rate ->
                    leg.Notional.Amount * rate * yf
                | None ->
                    0m

            Cashflow.create
                p.PaymentDate
                { Amount = amount
                  Currency = leg.Notional.Currency })

    let projector
        (roll: RollConvention)
        : CashflowProjector<InterestRateSwap> =

        fun swap ->

            let payLeg =
                projectFixedLeg roll swap.StartDate swap.MaturityDate swap.PayLeg

            let receiveLeg =
                projectFixedLeg roll swap.StartDate swap.MaturityDate swap.ReceiveLeg

            { Target = swap
              Cashflows = payLeg @ receiveLeg }
