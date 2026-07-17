namespace FSH.Modules.Chat.Contracts.v1.Dtos;

public sealed record MessageAttachmentDto(
    Guid Id,
    Guid? FileAssetId,
    string Url,
    string ContentType,
    string OriginalFileName,
    long SizeBytes);