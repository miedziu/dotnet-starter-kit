namespace FSH.Modules.Files.Contracts.v1.Dtos;

public sealed record PresignedUploadResponse(
    Guid FileAssetId,
    Uri UploadUrl,
    IReadOnlyDictionary<string, string> RequiredHeaders,
    DateTimeOffset ExpiresAt);