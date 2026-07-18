using Mediator;

namespace FSH.Mods.File.Spec.v1.File;

public sealed record FinalizeUploadCommand(Guid FileAssetId) : ICommand<FileAssetDto>;