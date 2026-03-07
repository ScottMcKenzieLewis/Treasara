namespace Treasara.Domain.Tests

open FluentAssertions
open NodaTime
open Treasara.Domain
open Xunit

module ScheduleTests =

    [<Fact>]
    let ``SemiAnnual schedule should generate two periods over one year`` () =
        let startDate = LocalDate(2026, 1, 1)
        let maturityDate = LocalDate(2027, 1, 1)

        let periods =
            Schedule.generate SemiAnnual NoRoll startDate maturityDate

        Assert.Equal(2, periods.Length)

        periods[0].AccrualStart.Should().Be(startDate)|> ignore
        periods[0].AccrualEnd.Should().Be(LocalDate(2026, 7, 1))|> ignore
        periods[1].AccrualStart.Should().Be(LocalDate(2026, 7, 1))|> ignore
        periods[1].AccrualEnd.Should().Be(maturityDate)

    [<Fact>]
    let ``EndOfMonth roll should preserve month-end semantics`` () =
        let startDate = LocalDate(2026, 1, 31)
        let maturityDate = LocalDate(2026, 4, 30)

        let periods =
            Schedule.generate Monthly EndOfMonth startDate maturityDate

        Assert.Equal(3, periods.Length)

        periods[0].AccrualEnd.Should().Be(LocalDate(2026, 2, 28))|> ignore
        periods[1].AccrualEnd.Should().Be(LocalDate(2026, 3, 31))|> ignore
        periods[2].AccrualEnd.Should().Be(LocalDate(2026, 4, 30))

    [<Fact>]
    let ``Schedule generation should throw when maturity is not after start`` () =
        let startDate = LocalDate(2026, 1, 1)

        Assert.Throws<DomainValidationException>(fun () ->
            Schedule.generate Annual NoRoll startDate startDate |> ignore)
        |> ignore