namespace FSH.Mod.File.Spec.v1.File;

public sealed record PresignedDownloadResponse(Uri Url, DateTimeOffset ExpiresAt);