using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Session;

public sealed record GetMySessionsQuery : IQuery<List<UserSessionDto>>;