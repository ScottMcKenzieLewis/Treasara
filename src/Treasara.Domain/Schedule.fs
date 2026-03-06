namespace Treasara.Domain

open NodaTime

type RollConvention =
    | NoRoll
    | EndOfMonth

type SchedulePeriod =
    { AccrualStart : LocalDate
      AccrualEnd : LocalDate
      PaymentDate : LocalDate }

module Schedule =

    let private isEndOfMonth (d: LocalDate) =
        d.Day = d.Calendar.GetDaysInMonth(d.Year, d.Month)

    let private addMonths (roll: RollConvention) (months: int) (d: LocalDate) =
        match roll with
        | NoRoll ->
            d.PlusMonths(months)
        | EndOfMonth ->
            if isEndOfMonth d then
                let shifted = d.PlusMonths(months)
                let lastDay = shifted.Calendar.GetDaysInMonth(shifted.Year, shifted.Month)
                LocalDate(shifted.Year, shifted.Month, lastDay)
            else
                d.PlusMonths(months)

    let generate
        (frequency: Frequency)
        (roll: RollConvention)
        (startDate: LocalDate)
        (maturityDate: LocalDate)
        : SchedulePeriod list =

        if maturityDate <= startDate then
            invalidArg "maturityDate" "Maturity must be after start date."

        let stepMonths = Frequency.months frequency

        let rec boundaries acc current =
            if current >= maturityDate then
                (maturityDate :: acc) |> List.rev
            else
                boundaries (current :: acc) (addMonths roll stepMonths current)

        let dates = boundaries [] startDate

        dates
        |> List.pairwise
        |> List.map (fun (a, b) ->
            { AccrualStart = a
              AccrualEnd = b
              PaymentDate = b })