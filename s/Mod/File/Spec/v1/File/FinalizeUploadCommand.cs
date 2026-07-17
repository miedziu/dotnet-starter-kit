using FSH.Modules.Files.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Files.Contracts.v1.Commands;

public sealed record FinalizeUploadCommand(Guid FileAssetId) : ICommand<FileAssetDto>;