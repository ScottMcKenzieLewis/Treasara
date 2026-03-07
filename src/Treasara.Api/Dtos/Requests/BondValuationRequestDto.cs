namespace Treasara.Api.Dtos.Requests;

/// <summary>
/// Request DTO for calculating bond valuations.
/// </summary>
/// <remarks>
/// This DTO contains all the parameters necessary to value a fixed-rate bond,
/// including the bond's characteristics (notional, coupon rate, dates, conventions)
/// and the market data (yield curve rate) required for present value calculations.
/// </remarks>
public sealed class BondValuationRequestDto
{
    /// <summary>
    /// Gets or sets the face value or principal amount of the bond.
    /// </summary>
    /// <value>
    /// A positive decimal representing the bond's notional amount.
    /// Must be greater than zero.
    /// </value>
    /// <example>1000000</example>
    public decimal Notional { get; set; }

    /// <summary>
    /// Gets or sets the currency in which the bond is denominated.
    /// </summary>
    /// <value>
    /// A three-letter currency code (case-insensitive).
    /// Valid values: "USD", "EUR", "GBP".
    /// Defaults to "USD".
    /// </value>
    /// <example>USD</example>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets the annual coupon rate as a decimal.
    /// </summary>
    /// <value>
    /// The annual coupon rate expressed as a decimal (e.g., 0.05 for 5%).
    /// Must be non-negative.
    /// </value>
    /// <example>0.05</example>
    public decimal CouponRate { get; set; }

    /// <summary>
    /// Gets or sets the date the bond was issued.
    /// </summary>
    /// <value>
    /// The bond's issue date. Must be before the maturity date.
    /// </value>
    /// <example>2024-01-01</example>
    public DateOnly IssueDate { get; set; }

    /// <summary>
    /// Gets or sets the date the bond matures and the principal is repaid.
    /// </summary>
    /// <value>
    /// The bond's maturity date. Must be after the issue date.
    /// </value>
    /// <example>2034-01-01</example>
    public DateOnly MaturityDate { get; set; }

    /// <summary>
    /// Gets or sets the frequency of coupon payments.
    /// </summary>
    /// <value>
    /// A string representing the payment frequency (case-insensitive).
    /// Valid values: "ANNUAL", "SEMIANNUAL", "QUARTERLY", "MONTHLY".
    /// Defaults to "SemiAnnual".
    /// </value>
    /// <example>SEMIANNUAL</example>
    public string Frequency { get; set; } = "SemiAnnual";

    /// <summary>
    /// Gets or sets the day count convention for calculating accrued interest.
    /// </summary>
    /// <value>
    /// A string representing the day count convention (case-insensitive).
    /// Valid values: "ACTUAL360", "ACTUAL365", "THIRTY360".
    /// Defaults to "Thirty360".
    /// </value>
    /// <example>THIRTY360</example>
    public string DayCount { get; set; } = "Thirty360";

    /// <summary>
    /// Gets or sets the flat yield curve rate used for discounting cash flows.
    /// </summary>
    /// <value>
    /// The continuously-compounded discount rate expressed as a decimal (e.g., 0.045 for 4.5%).
    /// This rate is applied uniformly to all maturities.
    /// </value>
    /// <example>0.045</example>
    public decimal CurveRate { get; set; }

    /// <summary>
    /// Gets or sets the roll convention for adjusting payment dates.
    /// </summary>
    /// <value>
    /// A string representing the roll convention (case-insensitive).
    /// Valid values: "NOROLL", "ENDOFMONTH".
    /// Defaults to "NoRoll".
    /// </value>
    /// <remarks>
    /// "ENDOFMONTH" adjusts dates to month-end when the issue date falls at month-end,
    /// ensuring consistent month-end payment dates throughout the bond's life.
    /// </remarks>
    /// <example>NOROLL</example>
    public string RollConvention { get; set; } = "NoRoll";

    /// <summary>
    /// Gets or sets the date on which to perform the valuation.
    /// </summary>
    /// <value>
    /// The valuation date from which present value calculations are performed.
    /// Should typically be between the issue date and maturity date.
    /// </value>
    /// <example>2026-03-06</example>
    public DateOnly ValuationDate { get; set; }
}