namespace FSH.Modules.Files.Contracts.v1.Dtos;

public sealed record PresignedDownloadResponse(Uri Url, DateTimeOffset ExpiresAt);