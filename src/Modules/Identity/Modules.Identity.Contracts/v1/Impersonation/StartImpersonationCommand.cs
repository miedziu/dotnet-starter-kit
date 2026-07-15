using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Impersonation;

// DurationMinutes: requested token lifetime, capped server-side at
// StartImpersonationCommandValidator.MaxImpersonationMinutes (60); null → JwtOptions.AccessTokenMinutes.
public sealed record StartImpersonationCommand(
    string TargetUserId,
    string? Reason,
    int? DurationMinutes = null)
    : ICommand<ImpersonationResponse>;

public sealed record ImpersonationResponse(
string AccessToken,
DateTime AccessTokenExpiresAt,
string ActorUserId,
string ImpersonatedUserId);