using Mediator;
using System.Collections.ObjectModel;

namespace FSH.Mods.File.Spec.v1.File;

public sealed record ListMyFilesQuery(int Page = 1, int PageSize = 20) : IQuery<ReadOnlyCollection<FileAssetDto>>;