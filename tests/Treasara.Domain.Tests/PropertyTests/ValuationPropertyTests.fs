namespace Treasara.Domain.Tests

open FluentAssertions
open FsCheck.Xunit
open NodaTime
open Treasara.Domain

module ValuationPropertyTests =

    [<Property>]
    let ``Bond PV scales linearly with notional`` (baseAmount: int) (multiplier: int) =
        let a = decimal (max 1 (min (abs baseAmount) 10_000))
        let m = decimal (max 1 (min (abs multiplier) 20))

        let issueDate = LocalDate(2026, 1, 1)
        let maturityDate = LocalDate(2027, 1, 1)
        let valuationDate = issueDate
        let curve = Flat 0.04m

        let bond1 =
            Bond.create
                (Notional.create a USD)
                0.05m
                issueDate
                maturityDate
                SemiAnnual
                Thirty360

        let bond2 =
            Bond.create
                (Notional.create (a * m) USD)
                0.05m
                issueDate
                maturityDate
                SemiAnnual
                Thirty360

        let pv1 =
            Pricing.pvBond valuationDate Thirty360 curve NoRoll bond1
            |> Option.get
            |> fun mny -> mny.Amount

        let pv2 =
            Pricing.pvBond valuationDate Thirty360 curve NoRoll bond2
            |> Option.get
            |> fun mny -> mny.Amount
        let difference = abs (pv2 - (pv1 * m))
        difference.Should().BeLessThan(0.0001m) |> ignore