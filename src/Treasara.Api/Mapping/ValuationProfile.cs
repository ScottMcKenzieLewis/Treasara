using AutoMapper;
using Treasara.Api.Dtos.Responses;
using Treasara.Domain;

namespace Treasara.Api.Mapping;

/// <summary>
/// AutoMapper profile for mapping valuation domain objects to their corresponding DTOs.
/// </summary>
/// <remarks>
/// This profile defines the mapping configurations between domain valuation results
/// and API response DTOs, including conversions for dates, money values, and currency codes.
/// The mappings handle the translation from NodaTime LocalDate to DateOnly and from
/// domain Money types to separate amount and currency string fields.
/// </remarks>
public sealed class ValuationProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValuationProfile"/> class and configures all mappings.
    /// </summary>
    /// <remarks>
    /// Configures the following mappings:
    /// <list type="bullet">
    /// <item><description>ValuationLine to ValuationLineDto with date and money conversions</description></item>
    /// <item><description>ValuationResult&lt;Bond&gt; to BondValuationResponseDto with aggregated values</description></item>
    /// </list>
    /// </remarks>
    public ValuationProfile()
    {
        CreateMap<ValuationLine, ValuationLineDto>()
            .ForMember(
                d => d.PaymentDate,
                opt => opt.MapFrom(s => new DateOnly(
                    s.PaymentDate.Year,
                    s.PaymentDate.Month,
                    s.PaymentDate.Day)))
            .ForMember(
                d => d.CashflowAmount,
                opt => opt.MapFrom(s => s.CashflowAmount.Amount))
            .ForMember(
                d => d.PresentValue,
                opt => opt.MapFrom(s => s.PresentValue.Amount))
            .ForMember(
                d => d.Currency,
                opt => opt.MapFrom(s => s.PresentValue.Currency.ToString()));

        CreateMap<ValuationResult<Bond>, BondValuationResponseDto>()
            .ForMember(
                d => d.InstrumentType,
                opt => opt.MapFrom(_ => "Bond"))
            .ForMember(
                d => d.TotalPresentValue,
                opt => opt.MapFrom(s => GetTotalPresentValueAmount(s)))
            .ForMember(
                d => d.Currency,
                opt => opt.MapFrom(s => GetTotalPresentValueCurrency(s)))
            .ForMember(
                d => d.Lines,
                opt => opt.MapFrom(s => s.Lines));
    }

    /// <summary>
    /// Extracts the total present value amount from a bond valuation result.
    /// </summary>
    /// <param name="source">The valuation result containing the bond and its calculated values.</param>
    /// <returns>
    /// The total present value as a decimal if available; otherwise, <c>null</c> if no valuation exists.
    /// </returns>
    /// <remarks>
    /// This helper method handles the nullable Money type by extracting only the Amount property
    /// when a value is present, or returning null to indicate the absence of a valuation.
    /// </remarks>
    private static decimal? GetTotalPresentValueAmount(ValuationResult<Bond> source)
    {
        return source.TotalPresentValue == null
            ? null
            : source.TotalPresentValue.Value.Amount;
    }

    /// <summary>
    /// Extracts the currency code from a bond valuation result's total present value.
    /// </summary>
    /// <param name="source">The valuation result containing the bond and its calculated values.</param>
    /// <returns>
    /// The currency code as a string (e.g., "USD", "EUR", "GBP") if a valuation exists;
    /// otherwise, an empty string.
    /// </returns>
    /// <remarks>
    /// This helper method ensures that the currency information is consistently provided
    /// in the response DTO, even when no valuation is available (represented by an empty string).
    /// </remarks>
    private static string GetTotalPresentValueCurrency(ValuationResult<Bond> source)
    {
        return source.TotalPresentValue == null
            ? string.Empty
            : source.TotalPresentValue.Value.Currency.ToString();
    }
}