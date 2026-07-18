using Mediator;

namespace FSH.Mods.Identity.Spec.v1.User;

public sealed record GetCurrentUserProfileQuery(string UserId) : IQuery<UserDto>;