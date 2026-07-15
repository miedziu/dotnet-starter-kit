using FSH.Modules.Identity.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Tokens;

public record GenerateTokenCommand(
    string Email,
    string Password,
    string? TwoFactorCode = null)
    : ICommand<TokenResponse>;