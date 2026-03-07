namespace Treasara.Domain.Tests

open FluentAssertions
open NodaTime
open Treasara.Domain
open Xunit

module SwapTests =

    let private usd amount = Notional.create amount USD
    let private eur amount = Notional.create amount EUR

    [<Fact>]
    let ``createFixedLeg should create fixed leg`` () =
        let leg =
            Swap.createFixedLeg
                (usd 1_000_000m)
                0.05m
                SemiAnnual
                Actual360

        leg.LegType.Should().Be(Fixed) |> ignore
        leg.FixedRate.Should().Be(Some 0.05m) |> ignore
        leg.FloatIndex.IsNone.Should().BeTrue() |> ignore

    [<Fact>]
    let ``createFloatingLeg should create floating leg`` () =
        let leg =
            Swap.createFloatingLeg
                (usd 1_000_000m)
                SOFR
                Quarterly
                Actual360

        leg.LegType.Should().Be(Floating) |> ignore
        leg.FixedRate.IsNone.Should().BeTrue() |> ignore
        leg.FloatIndex.Should().Be(Some SOFR) |> ignore

    [<Fact>]
    let ``createFixedLeg should reject negative rate`` () =
        Assert.Throws<DomainValidationException>(fun () ->
            Swap.createFixedLeg
                (usd 1_000_000m)
                -0.01m
                SemiAnnual
                Actual360
            |> ignore)
        |> ignore

    [<Fact>]
    let ``Swap create should reject maturity on or before start`` () =
        let payLeg = Swap.createFixedLeg (usd 1_000_000m) 0.05m SemiAnnual Actual360
        let receiveLeg = Swap.createFloatingLeg (usd 1_000_000m) SOFR Quarterly Actual360
        let date = LocalDate(2026, 1, 1)

        Assert.Throws<DomainValidationException>(fun () ->
            Swap.create date date payLeg receiveLeg |> ignore)
        |> ignore

    [<Fact>]
    let ``Swap create should reject different currencies`` () =
        let payLeg = Swap.createFixedLeg (usd 1_000_000m) 0.05m SemiAnnual Actual360
        let receiveLeg = Swap.createFloatingLeg (eur 1_000_000m) SOFR Quarterly Actual360

        Assert.Throws<DomainValidationException>(fun () ->
            Swap.create
                (LocalDate(2026, 1, 1))
                (LocalDate(2031, 1, 1))
                payLeg
                receiveLeg
            |> ignore)
        |> ignore

    [<Fact>]
    let ``Swap create should reject different notionals in v1`` () =
        let payLeg = Swap.createFixedLeg (usd 1_000_000m) 0.05m SemiAnnual Actual360
        let receiveLeg = Swap.createFloatingLeg (usd 2_000_000m) SOFR Quarterly Actual360

        Assert.Throws<DomainValidationException>(fun () ->
            Swap.create
                (LocalDate(2026, 1, 1))
                (LocalDate(2031, 1, 1))
                payLeg
                receiveLeg
            |> ignore)
        |> ignore