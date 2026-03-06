namespace Treasara.Domain.Tests

open FluentAssertions
open FsCheck.Xunit
open NodaTime
open Treasara.Domain

module YieldCurvePropertyTests =

    [<Property>]
    let ``Discount factor is always between 0 and 1 for non-negative rates`` (rate: decimal) (days: int) =
        let safeRate =
            abs rate % 1.0m

        let safeDays =
            max 1 (abs days % 3650)

        let valuationDate = LocalDate(2026, 1, 1)
        let paymentDate = valuationDate.PlusDays(safeDays)

        let df =
            YieldCurve.discountFactor
                Actual365
                valuationDate
                paymentDate
                (Flat safeRate)

        df.Should().BeGreaterThan(0m)|> ignore
        df.Should().BeLessThanOrEqualTo(1.0m) |> ignore