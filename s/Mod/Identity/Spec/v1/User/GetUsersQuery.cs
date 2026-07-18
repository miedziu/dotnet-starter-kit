using Mediator;

namespace FSH.Mod.Identity.Spec.v1.User;

public sealed record GetUsersQuery : IQuery<List<UserDto>>;