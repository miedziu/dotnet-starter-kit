using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Group;

public sealed record GetGroupByIdQuery(Guid Id) : IQuery<GroupDto>;