namespace Treasara.Domain

open NodaTime

/// <summary>
/// Defines conventions for adjusting payment dates when they fall on invalid business days or month boundaries.
/// </summary>
/// <remarks>
/// Roll conventions are critical in fixed-income instruments to handle edge cases when generating
/// payment schedules, particularly for bonds with payment dates near month-end.
/// </remarks>
type RollConvention =
    /// <summary>No date adjustment is applied; dates are calculated exactly as specified.</summary>
    | NoRoll
    /// <summary>If the start date is at month-end, all subsequent dates are adjusted to the end of their respective months.</summary>
    | EndOfMonth

/// <summary>
/// Represents a single period in a payment schedule, defining the accrual interval and payment timing.
/// </summary>
/// <remarks>
/// Schedule periods are used to define when interest accrues and when payments are made,
/// which is essential for calculating coupon payments and other periodic cash flows.
/// </remarks>
type SchedulePeriod =
    { /// <summary>The date when interest accrual begins for this period.</summary>
      AccrualStart: LocalDate
      /// <summary>The date when interest accrual ends for this period.</summary>
      AccrualEnd: LocalDate
      /// <summary>The date when the payment for this period is due.</summary>
      PaymentDate: LocalDate }

/// <summary>
/// Module containing functions for generating payment schedules for fixed-income instruments.
/// </summary>
module Schedule =

    /// <summary>
    /// Determines whether a given date falls on the last day of its month.
    /// </summary>
    /// <param name="d">The date to check.</param>
    /// <returns>True if the date is the last day of the month; otherwise, false.</returns>
    let private isEndOfMonth (d: LocalDate) =
        d.Day = d.Calendar.GetDaysInMonth(d.Year, d.Month)

    /// <summary>
    /// Adds a specified number of months to a date, applying the roll convention if applicable.
    /// </summary>
    /// <param name="roll">The roll convention to apply when adjusting dates.</param>
    /// <param name="months">The number of months to add.</param>
    /// <param name="d">The starting date.</param>
    /// <returns>The adjusted date after adding the specified months.</returns>
    /// <remarks>
    /// When the EndOfMonth convention is used and the starting date is at month-end,
    /// the result will be adjusted to the last day of the target month. This ensures
    /// consistency in payment schedules for instruments with month-end payment dates.
    /// </remarks>
    let private addMonths (roll: RollConvention) (months: int) (d: LocalDate) =
        match roll with
        | NoRoll -> d.PlusMonths(months)
        | EndOfMonth ->
            if isEndOfMonth d then
                let shifted = d.PlusMonths(months)
                let lastDay = shifted.Calendar.GetDaysInMonth(shifted.Year, shifted.Month)
                LocalDate(shifted.Year, shifted.Month, lastDay)
            else
                d.PlusMonths(months)

    /// <summary>
    /// Generates a complete payment schedule between a start date and maturity date.
    /// </summary>
    /// <param name="frequency">The payment frequency (e.g., Annual, SemiAnnual, Quarterly).</param>
    /// <param name="roll">The roll convention to apply for date adjustments.</param>
    /// <param name="startDate">The schedule start date (typically the bond issue date).</param>
    /// <param name="maturityDate">The schedule end date (typically the bond maturity date).</param>
    /// <returns>
    /// A list of schedule periods, each representing an accrual period and payment date.
    /// The periods cover the entire time span from start to maturity.
    /// </returns>
    /// <exception cref="DomainValidationException">
    /// Thrown when the maturity date is not after the start date, as a valid schedule
    /// requires a positive time span.
    /// </exception>
    /// <remarks>
    /// The function generates a schedule by:
    /// <list type="number">
    /// <item><description>Validating that maturity is after the start date</description></item>
    /// <item><description>Calculating the step size in months based on the frequency</description></item>
    /// <item><description>Recursively building boundary dates from start to maturity</description></item>
    /// <item><description>Creating periods from consecutive date pairs</description></item>
    /// </list>
    /// Each period's payment date is set to the end of its accrual period, which is standard
    /// practice for fixed-income instruments. The roll convention is applied when calculating
    /// each boundary date to ensure consistent handling of month-end dates.
    /// </remarks>
    let generate
        (frequency: Frequency)
        (roll: RollConvention)
        (startDate: LocalDate)
        (maturityDate: LocalDate)
        : SchedulePeriod list =

        if maturityDate <= startDate then
            raise (DomainValidationException("Maturity must be after start date."))

        let stepMonths = Frequency.months frequency

        let rec boundaries acc current =
            if current >= maturityDate then
                (maturityDate :: acc) |> List.rev
            else
                boundaries (current :: acc) (addMonths roll stepMonths current)

        let dates = boundaries [] startDate

        dates
        |> List.pairwise
        |> List.map (fun (a, b) ->
            { AccrualStart = a
              AccrualEnd = b
              PaymentDate = b })