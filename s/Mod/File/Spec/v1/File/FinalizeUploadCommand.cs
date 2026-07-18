using Mediator;

namespace FSH.Mod.File.Spec.v1.File;

public sealed record FinalizeUploadCommand(Guid FileAssetId) : ICommand<FileAssetDto>;