namespace Treasara.Api.Dtos.Responses;

public sealed class BondValuationResponseDto
{
    public string InstrumentType { get; set; } = "Bond";
    public decimal? TotalPresentValue { get; set; }
    public string Currency { get; set; } = string.Empty;
    public List<ValuationLineDto> Lines { get; set; } = [];
}