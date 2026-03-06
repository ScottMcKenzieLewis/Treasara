namespace Treasara.Domain

module InstrumentProjection =

    let projector
        (roll: RollConvention)
        (instrument: Instrument)
        : ProjectedCashflows<Instrument> =

        match instrument with

        | Bond bond ->

            let p = BondProjection.projector roll

            let result = p bond

            { Target = Instrument.Bond bond
              Cashflows = result.Cashflows }

        | Swap swap ->

            let p = SwapProjection.projector roll

            let result = p swap

            { Target = Instrument.Swap swap
              Cashflows = result.Cashflows }