using FluentValidation;
using FSH.Modules.Identity.Contracts.v1.Users.RegisterUserStep1;

namespace FSH.Modules.Identity.Features.v1.Users.RegisterUserStep1;

public sealed class RegisterUserStep1Validator : AbstractValidator<RegisterUserStep1Command>
{
    public RegisterUserStep1Validator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Password confirmation is required.")
            .Equal(x => x.Password).WithMessage("Passwords do not match.");

        RuleFor(x => x.ReferralUsernames)
            .Must(usernames => usernames is null || usernames.All(username => !string.IsNullOrWhiteSpace(username)))
            .WithMessage("Referral usernames must not contain empty or whitespace values.")
            .When(x => x.ReferralUsernames is not null && x.ReferralUsernames.Length > 0);
    }
}