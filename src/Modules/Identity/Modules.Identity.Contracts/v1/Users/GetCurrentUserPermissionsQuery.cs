using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Users;

public sealed record GetCurrentUserPermissionsQuery(string UserId) : IQuery<List<string>?>;