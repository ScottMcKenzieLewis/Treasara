namespace Treasara.Domain

open NodaTime

/// <summary>
/// Represents a fixed-income bond instrument with its core characteristics.
/// </summary>
/// <remarks>
/// A bond is a debt security that pays periodic coupon payments based on a fixed rate
/// and returns the notional amount at maturity. This record contains all the essential
/// parameters needed for bond valuation and cash flow projection.
/// </remarks>
type Bond =
    { /// <summary>The face value or principal amount of the bond.</summary>
      Notional: Notional
      /// <summary>The annual coupon rate expressed as a decimal (e.g., 0.05 for 5%).</summary>
      CouponRate: decimal
      /// <summary>The date the bond was issued.</summary>
      IssueDate: LocalDate
      /// <summary>The date the bond matures and the notional is repaid.</summary>
      MaturityDate: LocalDate
      /// <summary>The frequency of coupon payments (e.g., Annual, SemiAnnual).</summary>
      Frequency: Frequency
      /// <summary>The day count convention used for interest calculations.</summary>
      DayCount: DayCountConvention }

/// <summary>
/// Module containing functions for creating and validating Bond instances.
/// </summary>
module Bond =

    /// <summary>
    /// Validates a bond's business rules and constraints.
    /// </summary>
    /// <param name="bond">The bond to validate.</param>
    /// <returns>The validated bond if all rules pass.</returns>
    /// <exception cref="DomainValidationException">
    /// Thrown when:
    /// <list type="bullet">
    /// <item><description>Notional amount is not positive</description></item>
    /// <item><description>Coupon rate is negative</description></item>
    /// <item><description>Maturity date is not after issue date</description></item>
    /// </list>
    /// </exception>
    let private validate (bond: Bond) =
        if bond.Notional.Amount <= 0m then
            raise (DomainValidationException("Notional must be positive."))

        if bond.CouponRate < 0m then
            raise (DomainValidationException("CouponRate cannot be negative."))

        if bond.MaturityDate <= bond.IssueDate then
            raise (DomainValidationException("MaturityDate must be after IssueDate."))

        bond

    /// <summary>
    /// Creates a new bond instance with the specified parameters.
    /// </summary>
    /// <param name="notional">The face value of the bond.</param>
    /// <param name="couponRate">The annual coupon rate as a decimal.</param>
    /// <param name="issueDate">The date the bond was issued.</param>
    /// <param name="maturityDate">The date the bond matures.</param>
    /// <param name="frequency">The frequency of coupon payments.</param>
    /// <param name="dayCount">The day count convention for interest calculations.</param>
    /// <returns>A validated Bond instance.</returns>
    /// <exception cref="DomainValidationException">
    /// Thrown if any validation rules fail.
    /// </exception>
    /// <remarks>
    /// The bond is automatically validated upon creation to ensure all business rules
    /// are satisfied before returning the instance.
    /// </remarks>
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