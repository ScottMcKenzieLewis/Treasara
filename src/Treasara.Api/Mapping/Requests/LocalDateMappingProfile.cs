using AutoMapper;
using NodaTime;

namespace Treasara.Api.Mapping.Requests;

/// <summary>
/// AutoMapper profile for converting between .NET DateOnly and NodaTime LocalDate types.
/// </summary>
/// <remarks>
/// This profile enables seamless mapping between the standard .NET <see cref="DateOnly"/> type
/// used in API DTOs and the NodaTime <see cref="LocalDate"/> type used in the domain layer.
/// NodaTime is used in the domain for more robust and explicit date/time handling, avoiding
/// the ambiguities and pitfalls of standard .NET DateTime types.
/// </remarks>
public sealed class LocalDateMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalDateMappingProfile"/> class
    /// and configures the DateOnly to LocalDate mapping.
    /// </summary>
    /// <remarks>
    /// The mapping performs a straightforward component-wise conversion, extracting the
    /// Year, Month, and Day values from DateOnly and constructing a LocalDate with the
    /// same values. This conversion is safe because both types represent calendar dates
    /// without time or time zone information.
    /// </remarks>
    public LocalDateMappingProfile()
    {
        CreateMap<DateOnly, LocalDate>()
            .ConvertUsing(d => new LocalDate(d.Year, d.Month, d.Day));
    }
}
