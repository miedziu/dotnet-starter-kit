namespace FSH.Modules.Identity.Contracts.v1.Dtos;

public record TokenDto(string Token, string RefreshToken, DateTime RefreshTokenExpiryTime);