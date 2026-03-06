namespace Treasara.Domain.Tests

open FluentAssertions
open NodaTime
open Treasara.Domain
open Xunit

module YieldCurveTests =

    [<Fact>]
    let ``Discount factor should be 1 at valuation date`` () =
        let valuationDate = LocalDate(2026, 1, 1)

        let df =
            YieldCurve.discountFactor
                Actual365
                valuationDate
                valuationDate
                (Flat 0.05m)

        df.Should().Be(1.0m)

    [<Fact>]
    let ``Discount factor should decrease as payment date moves further out`` () =
        let valuationDate = LocalDate(2026, 1, 1)

        let oneYear =
            YieldCurve.discountFactor
                Actual365
                valuationDate
                (LocalDate(2027, 1, 1))
                (Flat 0.05m)

        let twoYear =
            YieldCurve.discountFactor
                Actual365
                valuationDate
                (LocalDate(2028, 1, 1))
                (Flat 0.05m)

        twoYear.Should().BeLessThan(oneYear)