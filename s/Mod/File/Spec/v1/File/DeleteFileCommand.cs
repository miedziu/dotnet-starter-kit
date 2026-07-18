using Mediator;

namespace FSH.Mod.File.Spec.v1.File;

public sealed record DeleteFileCommand(Guid FileAssetId) : ICommand<Unit>;