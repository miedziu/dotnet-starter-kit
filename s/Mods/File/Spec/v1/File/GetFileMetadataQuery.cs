using Mediator;

namespace FSH.Mods.File.Spec.v1.File;

public sealed record GetFileMetadataQuery(Guid FileAssetId) : IQuery<FileAssetDto>;