using FSH.Modules.Identity.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Users;

public sealed record GetUsersQuery : IQuery<List<UserDto>>;