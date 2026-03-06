namespace Treasara.Domain.Tests

open FluentAssertions
open NodaTime
open Treasara.Domain
open Xunit

module BondProjectionTests =

    [<Fact>]
    let ``BondProjection projector should return projected cashflows`` () =
        let bond =
            Bond.create
                (Notional.create 1_000m USD)
                0.05m
                (LocalDate(2026, 1, 1))
                (LocalDate(2027, 1, 1))
                SemiAnnual
                Thirty360

        let projector = BondProjection.projector NoRoll
        let result = projector bond

        result.Target.Should().Be(bond) |> ignore
        result.Cashflows.Length.Should().Be(3) |> ignore