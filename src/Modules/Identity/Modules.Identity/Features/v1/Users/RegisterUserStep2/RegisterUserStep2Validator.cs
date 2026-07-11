using FluentValidation;
using FSH.Modules.Identity.Contracts.v1.Users.RegisterUserStep2;

namespace FSH.Modules.Identity.Features.v1.Users.RegisterUserStep2;

public sealed class RegisterUserStep2Validator : AbstractValidator<RegisterUserStep2Command>
{
    public RegisterUserStep2Validator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.DistrictId)
            .Must(v => !v.HasValue || v.Value > 0).WithMessage("District ID must be greater than 0 if provided.")
            .When(x => x.DistrictId.HasValue);

        RuleFor(x => x.CommuneId)
            .Must(v => !v.HasValue || v.Value > 0).WithMessage("Commune ID must be greater than 0 if provided.")
            .When(x => x.CommuneId.HasValue);
    }
}