using Mediator;

namespace FSH.Modules.Identity.Contracts.v1.Groups;

public sealed record DeleteGroupCommand(Guid Id) : ICommand<Unit>;