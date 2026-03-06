namespace Treasara.Domain.Tests

open FluentAssertions
open FsCheck
open FsCheck.Xunit
open NodaTime
open Treasara.Domain

module BondPropertyTests =

    [<Property>]
    let ``Annual bond generates one coupon per year plus principal`` (years: int) =
        let y = max 1 (min (abs years) 20)

        let bond =
            Bond.create
                (Notional.create 1_000m USD)
                0.05m
                (LocalDate(2026, 1, 1))
                (LocalDate(2026 + y, 1, 1))
                Annual
                Thirty360

        let cashflows =
            BondCashflows.generate NoRoll bond

        cashflows.Length.Should().Be(y + 1) |> ignore