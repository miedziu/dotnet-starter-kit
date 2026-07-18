namespace FSH.Mod.File.Spec.v1.File;

public sealed record PresignedUploadResponse(
    Guid FileAssetId,
    Uri UploadUrl,
    IReadOnlyDictionary<string, string> RequiredHeaders,
    DateTimeOffset ExpiresAt);