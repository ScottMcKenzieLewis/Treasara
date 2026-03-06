namespace Treasara.Domain

type Instrument =
    | Bond of Bond
    | Swap of InterestRateSwap