using Mediator;

namespace FSH.Mods.File.Spec.v1.File;

public sealed record RequestUploadUrlCommand(
    string OwnerType,
    Guid? OwnerId,
    string FileName,
    string ContentType,
    long SizeBytes,
    Visibility Visibility,
    string Category) : ICommand<PresignedUploadResponse>;