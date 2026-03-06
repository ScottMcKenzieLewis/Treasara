namespace Treasara.Api.Dtos.Requests;

public sealed class BondValuationRequestDto
{
    public decimal Notional { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal CouponRate { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly MaturityDate { get; set; }
    public string Frequency { get; set; } = "SemiAnnual";
    public string DayCount { get; set; } = "Thirty360";
    public decimal CurveRate { get; set; }
    public string RollConvention { get; set; } = "NoRoll";
    public DateOnly ValuationDate { get; set; }
}