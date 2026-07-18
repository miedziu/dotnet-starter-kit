using Mediator;

namespace FSH.Mods.Identity.Spec.v1.User;

public sealed record GetUserQuery(string Id) : IQuery<UserDto>;