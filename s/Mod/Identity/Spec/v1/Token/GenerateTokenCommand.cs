using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Token;

public record GenerateTokenCommand(
    string Email,
    string Password,
    string? TwoFactorCode = null)
    : ICommand<TokenResponse>;