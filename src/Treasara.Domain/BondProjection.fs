namespace Treasara.Domain

module BondProjection =

    let projector (roll: RollConvention) : CashflowProjector<Bond> =
        fun bond ->
            { Target = bond
              Cashflows = BondCashflows.generate roll bond }