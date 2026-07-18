using Mediator;

namespace FSH.Mod.Identity.Spec.v1.User;

public sealed record GetUserQuery(string Id) : IQuery<UserDto>;