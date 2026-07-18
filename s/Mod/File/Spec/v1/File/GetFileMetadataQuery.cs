using Mediator;

namespace FSH.Mod.File.Spec.v1.File;

public sealed record GetFileMetadataQuery(Guid FileAssetId) : IQuery<FileAssetDto>;