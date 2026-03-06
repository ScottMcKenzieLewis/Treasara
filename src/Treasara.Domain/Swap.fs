namespace Treasara.Domain

open NodaTime

type FloatIndex =
    | SOFR
    | LIBOR3M
    | EURIBOR6M

type SwapLegType =
    | Fixed
    | Floating

type SwapLeg =
    { LegType : SwapLegType
      Notional : Notional
      FixedRate : decimal option
      FloatIndex : FloatIndex option
      Frequency : Frequency
      DayCount : DayCountConvention }

type InterestRateSwap =
    { StartDate : LocalDate
      MaturityDate : LocalDate
      PayLeg : SwapLeg
      ReceiveLeg : SwapLeg }

module Swap =

    let private validateLeg (leg: SwapLeg) =
        if leg.Notional.Amount <= 0m then
            invalidArg "leg" "Swap leg notional must be positive."

        match leg.LegType, leg.FixedRate, leg.FloatIndex with
        | Fixed, None, _ ->
            invalidArg "leg" "Fixed leg must have a fixed rate."
        | Fixed, Some rate, _ when rate < 0m ->
            invalidArg "leg" "Fixed leg rate cannot be negative."
        | Fixed, Some _, Some _ ->
            invalidArg "leg" "Fixed leg cannot also have a floating index."
        | Floating, Some _, _ ->
            invalidArg "leg" "Floating leg cannot have a fixed rate."
        | Floating, _, None ->
            invalidArg "leg" "Floating leg must have a floating index."
        | _ ->
            leg

    let private validate (swap: InterestRateSwap) =
        if swap.MaturityDate <= swap.StartDate then
            invalidArg "swap" "MaturityDate must be after StartDate."

        validateLeg swap.PayLeg |> ignore
        validateLeg swap.ReceiveLeg |> ignore

        if swap.PayLeg.Notional.Currency <> swap.ReceiveLeg.Notional.Currency then
            invalidArg "swap" "Pay and receive legs must have the same currency."

        if swap.PayLeg.Notional.Amount <> swap.ReceiveLeg.Notional.Amount then
            invalidArg "swap" "Pay and receive legs must have the same notional in v1."

        swap

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