using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Group;

public sealed record GetGroupsQuery(string? SearchTerm = null) : IQuery<IEnumerable<GroupDto>>;