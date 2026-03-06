namespace Treasara.Domain.Tests

open FluentAssertions
open NodaTime
open Treasara.Domain
open Xunit

module BondCashflowTests =

    let private usdNotional amount =
        Notional.create amount USD

    [<Fact>]
    let ``Bond should generate coupon cashflows plus principal`` () =
        let bond =
            Bond.create
                (usdNotional 1_000_000m)
                0.05m
                (LocalDate(2026, 1, 1))
                (LocalDate(2027, 1, 1))
                SemiAnnual
                Thirty360

        let cashflows =
            BondCashflows.generate NoRoll bond

        Assert.Equal(3, cashflows.Length)

        cashflows[0].Amount.Amount.Should().Be(25_000m)|> ignore
        cashflows[1].Amount.Amount.Should().Be(25_000m)|> ignore
        cashflows[2].Amount.Amount.Should().Be(1_000_000m)|> ignore

    [<Fact>]
    let ``Final bond cashflow should occur on maturity date`` () =
        let maturity = LocalDate(2027, 1, 1)

        let bond =
            Bond.create
                (usdNotional 100m)
                0.06m
                (LocalDate(2026, 1, 1))
                maturity
                SemiAnnual
                Thirty360

        let cashflows =
            BondCashflows.generate NoRoll bond

        let lastCashflow = List.last cashflows
        lastCashflow.PaymentDate.Should().Be(maturity)