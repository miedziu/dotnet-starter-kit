using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Session;

public sealed record GetUserSessionsQuery(Guid UserId) : IQuery<List<UserSessionDto>>;