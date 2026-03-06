namespace Treasara.Domain

open NodaTime

type Bond =
    { Notional : Notional
      CouponRate : decimal
      IssueDate : LocalDate
      MaturityDate : LocalDate
      Frequency : Frequency
      DayCount : DayCountConvention }

module Bond =

    let private validate (bond: Bond) =
        if bond.Notional.Amount <= 0m then
            invalidArg "bond" "Notional must be positive."

        if bond.CouponRate < 0m then
            invalidArg "bond" "CouponRate cannot be negative."

        if bond.MaturityDate <= bond.IssueDate then
            invalidArg "bond" "MaturityDate must be after IssueDate."

        bond

    let create
        (notional: Notional)
        (couponRate: decimal)
        (issueDate: LocalDate)
        (maturityDate: LocalDate)
        (frequency: Frequency)
        (dayCount: DayCountConvention)
        : Bond =
        { Notional = notional
          CouponRate = couponRate
          IssueDate = issueDate
          MaturityDate = maturityDate
          Frequency = frequency
          DayCount = dayCount }
        |> validate