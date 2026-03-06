namespace Treasara.Domain.Tests

open FluentAssertions
open NodaTime
open Treasara.Domain
open Xunit

module TimeTests =

    [<Fact>]
    let ``Actual360 should return 0.5 for 180 days`` () =
        let startDate = LocalDate(2026, 1, 1)
        let endDate = startDate.PlusDays(180)

        let result = DayCount.yearFraction Actual360 startDate endDate

        result.Should().Be(0.5m)

    [<Fact>]
    let ``Actual365 should return 1.0 for 365 days`` () =
        let startDate = LocalDate(2026, 1, 1)
        let endDate = startDate.PlusDays(365)

        let result = DayCount.yearFraction Actual365 startDate endDate

        result.Should().Be(1.0m)

    [<Fact>]
    let ``Thirty360 should return 0.5 for Jan 1 to Jul 1`` () =
        let startDate = LocalDate(2026, 1, 1)
        let endDate = LocalDate(2026, 7, 1)

        let result = DayCount.yearFraction Thirty360 startDate endDate

        result.Should().Be(0.5m)

