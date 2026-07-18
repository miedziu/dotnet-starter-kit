namespace FSH.Mod.Identity.Spec.v1;

public record TokenDto(string Token, string RefreshToken, DateTime RefreshTokenExpiryTime);