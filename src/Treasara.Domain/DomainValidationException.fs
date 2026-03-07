namespace Treasara.Domain

open System

type DomainValidationException(message: string) =
    inherit Exception(message)