namespace FSH.Modules.Identity.Contracts.v1.Impersonation;

public sealed record ImpersonationResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string ActorUserId,
    string ImpersonatedUserId);