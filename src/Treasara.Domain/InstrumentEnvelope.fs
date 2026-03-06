namespace Treasara.Domain

type InstrumentEnvelope =
    { InstrumentType : System.Type
      Value : obj }

module InstrumentEnvelope =

    let create<'a> (value: 'a) =
        { InstrumentType = typeof<'a>
          Value = box value }

    let tryUnbox<'a> (envelope: InstrumentEnvelope) =
        match envelope.Value with
        | :? 'a as value -> Some value
        | _ -> None