using Mediator;

namespace FSH.Mods.Identity.Spec.v1.User;

public sealed record GetCurrentUserPermissionsQuery(string UserId) : IQuery<List<string>?>;