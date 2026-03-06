namespace Treasara.Domain.Tests

open FluentAssertions
open NodaTime
open Treasara.Domain
open Xunit

module ValuationTests =

    [<Fact>]
    let ``valueProjected should return one valuation line per cashflow`` () =
        let bond =
            Bond.create
                (Notional.create 1_000m USD)
                0.05m
                (LocalDate(2026, 1, 1))
                (LocalDate(2027, 1, 1))
                SemiAnnual
                Thirty360

        let projection =
            BondProjection.projector NoRoll bond

        let result =
            Valuation.valueProjected
                (LocalDate(2026, 1, 1))
                Thirty360
                (Flat 0.04m)
                projection

        result.Lines.Length.Should().Be(3) |> ignore
        result.TotalPresentValue.IsSome.Should().BeTrue() |> ignore
        result.Target.Should().Be(bond) |> ignore

    [<Fact>]
    let ``valueProjected should preserve currency in valuation lines`` () =
        let bond =
            Bond.create
                (Notional.create 1_000m USD)
                0.05m
                (LocalDate(2026, 1, 1))
                (LocalDate(2027, 1, 1))
                SemiAnnual
                Thirty360

        let projection =
            BondProjection.projector NoRoll bond

        let result =
            Valuation.valueProjected
                (LocalDate(2026, 1, 1))
                Thirty360
                (Flat 0.04m)
                projection

        result.Lines
        |> List.iter (fun line ->
            line.CashflowAmount.Currency.Should().Be(USD) |> ignore
            line.PresentValue.Currency.Should().Be(USD) |> ignore)

    [<Fact>]
    let ``valueProjected should discount future cashflows below nominal for positive rate`` () =
        let bond =
            Bond.create
                (Notional.create 1_000m USD)
                0.05m
                (LocalDate(2026, 1, 1))
                (LocalDate(2027, 1, 1))
                SemiAnnual
                Thirty360

        let projection =
            BondProjection.projector NoRoll bond

        let result =
            Valuation.valueProjected
                (LocalDate(2026, 1, 1))
                Thirty360
                (Flat 0.10m)
                projection

        let undiscountedTotal =
            projection.Cashflows
            |> List.sumBy (fun cf -> cf.Amount.Amount)

        result.TotalPresentValue.Value.Amount.Should().BeLessThan(undiscountedTotal) |> ignore

    [<Fact>]
    let ``valueWithProjector should value target using supplied projector`` () =
        let bond =
            Bond.create
                (Notional.create 1_000m USD)
                0.05m
                (LocalDate(2026, 1, 1))
                (LocalDate(2027, 1, 1))
                SemiAnnual
                Thirty360

        let result =
            Valuation.valueWithProjector
                (LocalDate(2026, 1, 1))
                Thirty360
                (Flat 0.04m)
                (BondProjection.projector NoRoll)
                bond

        result.Target.Should().Be(bond) |> ignore
        result.Lines.Length.Should().Be(3) |> ignore
        result.TotalPresentValue.IsSome.Should().BeTrue() |> ignore