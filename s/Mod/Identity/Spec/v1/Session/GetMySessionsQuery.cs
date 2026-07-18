using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Session;

public sealed record GetMySessionsQuery : IQuery<List<UserSessionDto>>;