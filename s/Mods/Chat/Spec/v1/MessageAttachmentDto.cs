namespace FSH.Mods.Chat.Spec.v1;

public sealed record MessageAttachmentDto(
    Guid Id,
    Guid? FileAssetId,
    string Url,
    string ContentType,
    string OriginalFileName,
    long SizeBytes);