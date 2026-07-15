using FSH.Modules.Identity.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Sessions;

public sealed record GetUserSessionsQuery(Guid UserId) : IQuery<List<UserSessionDto>>;