using FluentValidation;
using FSH.Modules.Identity.Contracts.v1.Users.RegisterUserStep2;

namespace FSH.Modules.Identity.Features.v1.Users.RegisterUserStep2;

public sealed class RegisterUserStep2Validator : AbstractValidator<RegisterUserStep2Command>
{
    public RegisterUserStep2Validator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.VoivodeshipId)
            .GreaterThan(0).WithMessage("Voivodeship is required.")
            .When(x => x.VoivodeshipId.HasValue);

        RuleFor(x => x.DistrictId)
            .GreaterThan(0).WithMessage("District is required.")
            .When(x => x.DistrictId.HasValue);

        RuleFor(x => x.CommuneId)
            .GreaterThan(0).WithMessage("Commune is required.")
            .When(x => x.CommuneId.HasValue);
    }
}