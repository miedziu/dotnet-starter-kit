using Mediator;

namespace FSH.Mods.Identity.Spec.v1.User;

public sealed record GetUsersQuery : IQuery<List<UserDto>>;