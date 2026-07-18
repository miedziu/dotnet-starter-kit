using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Mod.File.Spec.v1.File;

public sealed record ListTrashedFilesQuery(int PageNumber = 1, int PageSize = 20)
    : IQuery<PagedResponse<FileAssetDto>>;