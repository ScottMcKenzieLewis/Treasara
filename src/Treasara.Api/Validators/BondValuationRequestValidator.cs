using FluentValidation;
using Treasara.Api.Dtos.Requests;

namespace Treasara.Api.Validators;

/// <summary>
/// FluentValidation validator for bond valuation request DTOs.
/// </summary>
/// <remarks>
/// This validator enforces input validation rules before bond valuation requests are
/// processed by the domain layer. It validates that all required fields are provided,
/// numeric values are within acceptable ranges, and date relationships are logically
/// consistent. These validations help provide clear error messages to API consumers
/// and prevent invalid data from reaching the domain logic.
/// </remarks>
public sealed class BondValuationRequestValidator
    : AbstractValidator<BondValuationRequestDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BondValuationRequestValidator"/> class
    /// and configures all validation rules.
    /// </summary>
    /// <remarks>
    /// Validation rules enforced:
    /// <list type="bullet">
    /// <item><description>Notional must be greater than 0 (bonds require positive principal amounts)</description></item>
    /// <item><description>CouponRate must be greater than or equal to 0 (negative rates not allowed)</description></item>
    /// <item><description>Currency must not be empty (required for all monetary calculations)</description></item>
    /// <item><description>Frequency must not be empty (required for payment schedule generation)</description></item>
    /// <item><description>DayCount must not be empty (required for interest calculations)</description></item>
    /// <item><description>RollConvention must not be empty (required for date adjustments)</description></item>
    /// <item><description>MaturityDate must be after IssueDate (bonds cannot mature before issuance)</description></item>
    /// <item><description>ValuationDate must not be before IssueDate (cannot value a bond before it exists)</description></item>
    /// </list>
    /// Note: Specific value validation (e.g., valid currency codes like "USD", "EUR", "GBP")
    /// is performed by the controller mapping logic after these basic rules pass.
    /// </remarks>
    public BondValuationRequestValidator()
    {
        RuleFor(x => x.Notional)
            .GreaterThan(0)
            .WithMessage("Notional must be positive.");

        RuleFor(x => x.CouponRate)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Coupon rate cannot be negative.");

        RuleFor(x => x.Currency)
            .NotEmpty();

        RuleFor(x => x.Frequency)
            .NotEmpty();

        RuleFor(x => x.DayCount)
            .NotEmpty();

        RuleFor(x => x.RollConvention)
            .NotEmpty();

        RuleFor(x => x.MaturityDate)
            .GreaterThan(x => x.IssueDate)
            .WithMessage("MaturityDate must be after IssueDate.");

        RuleFor(x => x.ValuationDate)
            .GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("ValuationDate must not be before IssueDate.");
    }
}