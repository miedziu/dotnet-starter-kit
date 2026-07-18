using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Token;

// Token is the (possibly expired) access token, optional. When present, the handler cross-checks its
// subject against the refresh token's as a safeguard; when absent, refresh relies on refresh-token validation alone.
public record RefreshTokenCommand(string? Token, string RefreshToken)
    : ICommand<RefreshTokenCommandResponse>;

public sealed record RefreshTokenCommandResponse(
string Token,
string RefreshToken,
DateTime RefreshTokenExpiryTime);