namespace Treasara.Domain

open NUlid

type Id<'a> = Id of Ulid

module Id =

    let create<'a> () : Id<'a> =
        Id (Ulid.NewUlid())

    let value (Id id) = id

    let toString id =
        value id |> string