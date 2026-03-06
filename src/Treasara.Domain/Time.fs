namespace Treasara.Domain

open NodaTime

type ValuationDate = LocalDate

type DayCountConvention =
    | Actual360
    | Actual365
    | Thirty360

type Frequency =
    | Annual
    | SemiAnnual
    | Quarterly
    | Monthly

module Frequency =

    let months = function
        | Annual -> 12
        | SemiAnnual -> 6
        | Quarterly -> 3
        | Monthly -> 1

module DayCount =

    let yearFraction convention (startDate: LocalDate) (endDate: LocalDate) =
        let days =
            Period.Between(startDate, endDate, PeriodUnits.Days).Days
            |> decimal

        match convention with
        | Actual360 -> days / 360m
        | Actual365 -> days / 365m
        | Thirty360 ->
            let d1 = min startDate.Day 30
            let d2 = min endDate.Day 30

            let months =
                (endDate.Year - startDate.Year) * 12 +
                (endDate.Month - startDate.Month)

            let totalDays =
                (months * 30) + (d2 - d1)
                |> decimal

            totalDays / 360m