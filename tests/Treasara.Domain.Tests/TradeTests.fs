namespace Treasara.Domain.Tests

open FluentAssertions
open NodaTime
open Treasara.Domain
open Xunit

module TradeTests =

    [<Fact>]
    let ``Trade create should set properties`` () =
        let tradeId : TradeId = Id.create<TradeTag> ()

        let bond =
            Bond.create
                (Notional.create 1_000m USD)
                0.05m
                (LocalDate(2026, 1, 1))
                (LocalDate(2027, 1, 1))
                Annual
                Thirty360

        let trade =
            Trade.create
                tradeId
                (Instrument.Bond bond)
                (LocalDate(2026, 1, 2))
                None

        trade.TradeId.Should().Be(tradeId) |> ignore
        trade.TradeDate.Should().Be(LocalDate(2026, 1, 2)) |> ignore
        trade.Counterparty.IsNone.Should().BeTrue() |> ignore

    [<Fact>]
    let ``Trade withCounterparty should set counterparty`` () =
        let tradeId : TradeId = Id.create<TradeTag> ()
        let cp : CounterpartyId = Id.create<CounterpartyTag> ()

        let bond =
            Bond.create
                (Notional.create 1_000m USD)
                0.05m
                (LocalDate(2026, 1, 1))
                (LocalDate(2027, 1, 1))
                Annual
                Thirty360

        let trade =
            Trade.create
                tradeId
                (Instrument.Bond bond)
                (LocalDate(2026, 1, 2))
                None
            |> Trade.withCounterparty cp

        trade.Counterparty.IsSome.Should().BeTrue() |> ignore
        trade.Counterparty.Value.Should().Be(cp) |> ignore