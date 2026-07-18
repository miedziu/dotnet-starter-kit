using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Session;

public sealed record GetUserSessionsQuery(Guid UserId) : IQuery<List<UserSessionDto>>;