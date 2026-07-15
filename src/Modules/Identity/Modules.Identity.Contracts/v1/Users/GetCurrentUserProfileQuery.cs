using FSH.Modules.Identity.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Users;

public sealed record GetCurrentUserProfileQuery(string UserId) : IQuery<UserDto>;