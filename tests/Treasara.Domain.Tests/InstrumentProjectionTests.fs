namespace Treasara.Domain.Tests

open FluentAssertions
open NodaTime
open Treasara.Domain
open Xunit

module InstrumentProjectionTests =

    [<Fact>]
    let ``InstrumentProjection should project bond instrument`` () =
        let bond =
            Bond.create
                (Notional.create 1_000m USD)
                0.05m
                (LocalDate(2026, 1, 1))
                (LocalDate(2027, 1, 1))
                SemiAnnual
                Thirty360

        let result =
            InstrumentProjection.projector NoRoll (Instrument.Bond bond)

        result.Target.Should().Be(Instrument.Bond bond) |> ignore
        result.Cashflows.Length.Should().Be(3) |> ignore

    [<Fact>]
    let ``InstrumentProjection should project swap instrument`` () =
        let notional = Notional.create 1_000_000m USD

        let payLeg =
            Swap.createFixedLeg
                notional
                0.05m
                SemiAnnual
                Actual360

        let receiveLeg =
            Swap.createFloatingLeg
                notional
                SOFR
                Quarterly
                Actual360

        let swap =
            Swap.create
                (LocalDate(2026, 1, 1))
                (LocalDate(2027, 1, 1))
                payLeg
                receiveLeg

        let result =
            InstrumentProjection.projector NoRoll (Instrument.Swap swap)

        result.Target.Should().Be(Instrument.Swap swap) |> ignore
        result.Cashflows.Length.Should().BeGreaterThan(0) |> ignore