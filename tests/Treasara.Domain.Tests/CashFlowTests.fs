namespace Treasara.Domain.Tests

open FluentAssertions
open NodaTime
open Treasara.Domain
open Xunit

module CashflowMathTests =

    [<Fact>]
    let ``CashflowMath sumSameCurrency should return none for empty list`` () =
        let result = CashflowMath.sumSameCurrency []

        result.IsNone.Should().BeTrue() |> ignore

    [<Fact>]
    let ``CashflowMath sumSameCurrency should sum cashflows of same currency`` () =
        let paymentDate = LocalDate(2026, 1, 1)

        let cashflows =
            [ Cashflow.create paymentDate (Money.create 10m USD)
              Cashflow.create paymentDate (Money.create 15m USD) ]

        let result = CashflowMath.sumSameCurrency cashflows

        result.Value.Amount.Should().Be(25m) |> ignore
        result.Value.Currency.Should().Be(USD) |> ignore

    [<Fact>]
    let ``CashflowMath sumSameCurrency should throw for mixed currencies`` () =
        let paymentDate = LocalDate(2026, 1, 1)

        let cashflows =
            [ Cashflow.create paymentDate (Money.create 10m USD)
              Cashflow.create paymentDate (Money.create 15m EUR) ]

        Assert.Throws<DomainValidationException>(fun () -> CashflowMath.sumSameCurrency cashflows |> ignore)
        |> ignore