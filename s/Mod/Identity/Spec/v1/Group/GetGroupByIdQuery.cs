using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Group;

public sealed record GetGroupByIdQuery(Guid Id) : IQuery<GroupDto>;