using FSH.Framework.Shared.Persistence;
using FSH.Modules.Files.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Files.Contracts.v1.Queries;

public sealed record ListTrashedFilesQuery(int PageNumber = 1, int PageSize = 20)
    : IQuery<PagedResponse<FileAssetDto>>;