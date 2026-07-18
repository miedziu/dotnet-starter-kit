namespace FSH.Mods.Identity.Spec.v1.Token;

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    DateTime AccessTokenExpiresAt);