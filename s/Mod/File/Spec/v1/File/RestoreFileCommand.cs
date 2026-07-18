using Mediator;

namespace FSH.Mod.File.Spec.v1.File;

public sealed record RestoreFileCommand(Guid FileAssetId) : ICommand<Unit>;