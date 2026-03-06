namespace Treasara.Domain.Tests

open FluentAssertions
open Treasara.Domain
open Xunit

module MoneyTests =

    [<Fact>]
    let ``Money create should set amount and currency`` () =
        let money = Money.create 12.5m USD
        money.Amount.Should().Be(12.5m) |> ignore
        money.Currency.Should().Be(USD) |> ignore

    [<Fact>]
    let ``Money add should add same currency values`` () =
        let a = Money.create 10m USD
        let b = Money.create 5m USD

        let result = Money.add a b

        result.Amount.Should().Be(15m) |> ignore
        result.Currency.Should().Be(USD) |> ignore

    [<Fact>]
    let ``Money add should throw for mixed currencies`` () =
        let a = Money.create 10m USD
        let b = Money.create 5m EUR

        Assert.Throws<System.ArgumentException>(fun () -> Money.add a b |> ignore)
        |> ignore

    [<Fact>]
    let ``Money scale should multiply amount`` () =
        let money = Money.create 10m USD

        let result = Money.scale 2.5m money

        result.Amount.Should().Be(25m) |> ignore
        result.Currency.Should().Be(USD) |> ignore

    [<Fact>]
    let ``Money sumSameCurrency should return none for empty list`` () =
        let result = Money.sumSameCurrency []

        result.IsNone.Should().BeTrue() |> ignore

    [<Fact>]
    let ``Money sumSameCurrency should sum same currency list`` () =
        let monies =
            [ Money.create 10m USD
              Money.create 20m USD
              Money.create 5m USD ]

        let result = Money.sumSameCurrency monies

        result.Value.Amount.Should().Be(35m) |> ignore
        result.Value.Currency.Should().Be(USD) |> ignore

    [<Fact>]
    let ``Money sumSameCurrency should throw for mixed currencies`` () =
        let monies =
            [ Money.create 10m USD
              Money.create 20m EUR ]

        Assert.Throws<System.ArgumentException>(fun () -> Money.sumSameCurrency monies |> ignore)
        |> ignore