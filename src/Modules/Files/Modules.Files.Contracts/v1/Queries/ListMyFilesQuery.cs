using FSH.Modules.Files.Contracts.v1.DTOs;
using Mediator;
using System.Collections.ObjectModel;

namespace FSH.Modules.Files.Contracts.v1.Queries;

public sealed record ListMyFilesQuery(int Page = 1, int PageSize = 20) : IQuery<ReadOnlyCollection<FileAssetDto>>;