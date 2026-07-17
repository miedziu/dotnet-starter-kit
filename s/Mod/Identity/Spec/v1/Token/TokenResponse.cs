namespace FSH.Modules.Identity.Contracts.v1.Dtos;

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    DateTime AccessTokenExpiresAt);