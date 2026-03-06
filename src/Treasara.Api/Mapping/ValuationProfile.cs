using AutoMapper;
using Treasara.Api.Dtos.Responses;
using Treasara.Domain;

namespace Treasara.Api.Mapping;

public sealed class ValuationProfile : Profile
{
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

    private static decimal? GetTotalPresentValueAmount(ValuationResult<Bond> source)
    {
        return source.TotalPresentValue == null
            ? null
            : source.TotalPresentValue.Value.Amount;
    }

    private static string GetTotalPresentValueCurrency(ValuationResult<Bond> source)
    {
        return source.TotalPresentValue == null
            ? string.Empty
            : source.TotalPresentValue.Value.Currency.ToString();
    }
}