namespace Treasara.Domain

type ProjectedCashflows<'a> =
    { Target : 'a
      Cashflows : Cashflow list }

type CashflowProjector<'a> = 'a -> ProjectedCashflows<'a>

module Projection =

    let project (projector: CashflowProjector<'a>) (target: 'a) =
        projector target