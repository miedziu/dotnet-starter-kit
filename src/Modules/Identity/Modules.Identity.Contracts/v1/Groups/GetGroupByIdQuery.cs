using FSH.Modules.Identity.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Groups;

public sealed record GetGroupByIdQuery(Guid Id) : IQuery<GroupDto>;