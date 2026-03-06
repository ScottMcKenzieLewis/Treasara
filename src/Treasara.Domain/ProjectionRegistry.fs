namespace Treasara.Domain

open System
open System.Collections.Generic

type Projector =
    InstrumentEnvelope -> ProjectedCashflows<InstrumentEnvelope> option

type ProjectionRegistry =
    { Projectors : Dictionary<Type, Projector> }

module ProjectionRegistry =

    let empty () =
        { Projectors = Dictionary<Type, Projector>() }

    let register<'a>
        (projector: CashflowProjector<'a>)
        (registry: ProjectionRegistry) =

        let wrapped : Projector =
            fun envelope ->
                match InstrumentEnvelope.tryUnbox<'a> envelope with
                | Some target ->
                    let projection = projector target
                    Some
                        { Target = envelope
                          Cashflows = projection.Cashflows }
                | None ->
                    None

        registry.Projectors.[typeof<'a>] <- wrapped
        registry

    let tryProject
        (registry: ProjectionRegistry)
        (envelope: InstrumentEnvelope) =

        match registry.Projectors.TryGetValue envelope.InstrumentType with
        | true, projector -> projector envelope
        | false, _ -> None