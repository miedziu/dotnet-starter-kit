using Mediator;

namespace FSH.Mod.Identity.Spec.v1.User;

public sealed record GetCurrentUserPermissionsQuery(string UserId) : IQuery<List<string>?>;