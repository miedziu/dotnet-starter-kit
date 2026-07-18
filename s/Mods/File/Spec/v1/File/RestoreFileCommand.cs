using Mediator;

namespace FSH.Mods.File.Spec.v1.File;

public sealed record RestoreFileCommand(Guid FileAssetId) : ICommand<Unit>;