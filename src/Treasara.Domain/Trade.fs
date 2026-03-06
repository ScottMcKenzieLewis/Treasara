namespace Treasara.Domain

open NodaTime

type TradeTag = TradeTag
type CounterpartyTag = CounterpartyTag

type TradeId = Id<TradeTag>
type CounterpartyId = Id<CounterpartyTag>

type Trade =
    { TradeId : TradeId
      Instrument : Instrument
      TradeDate : LocalDate
      Counterparty : CounterpartyId option }

module Trade =

    let private validate (trade: Trade) =
        trade

    let create
        (tradeId: TradeId)
        (instrument: Instrument)
        (tradeDate: LocalDate)
        (counterparty: CounterpartyId option)
        : Trade =
        { TradeId = tradeId
          Instrument = instrument
          TradeDate = tradeDate
          Counterparty = counterparty }
        |> validate

    let withCounterparty (counterpartyId: CounterpartyId) (trade: Trade) =
        { trade with Counterparty = Some counterpartyId }

    let tradeId (trade: Trade) = trade.TradeId
    let instrument (trade: Trade) = trade.Instrument
    let tradeDate (trade: Trade) = trade.TradeDate
    let counterparty (trade: Trade) = trade.Counterparty