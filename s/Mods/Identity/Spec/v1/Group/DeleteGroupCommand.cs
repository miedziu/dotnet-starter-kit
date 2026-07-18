using Mediator;

namespace FSH.Mods.Identity.Spec.v1.Group;

public sealed record DeleteGroupCommand(Guid Id) : ICommand<Unit>;