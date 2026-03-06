namespace Treasara.Domain

module RegistryValuation =

    let value
        (registry: ProjectionRegistry)
        (valuationDate: ValuationDate)
        (dayCount: DayCountConvention)
        (curve: YieldCurve)
        (instrument: InstrumentEnvelope) =

        match ProjectionRegistry.tryProject registry instrument with
        | Some projection ->
            Valuation.valueProjected valuationDate dayCount curve projection
        | None ->
            invalidArg "instrument" $"No projector registered for type {instrument.InstrumentType.Name}."