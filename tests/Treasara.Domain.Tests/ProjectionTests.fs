namespace Treasara.Domain.Tests

open FluentAssertions
open NodaTime
open Treasara.Domain
open Xunit

module ProjectionTests =

    [<Fact>]
    let ``Projection project should invoke supplied projector`` () =
        let bond =
            Bond.create
                (Notional.create 1_000m USD)
                0.05m
                (LocalDate(2026, 1, 1))
                (LocalDate(2027, 1, 1))
                SemiAnnual
                Thirty360

        let result =
            Projection.project (BondProjection.projector NoRoll) bond

        result.Target.Should().Be(bond) |> ignore
        result.Cashflows.Length.Should().Be(3) |> ignore