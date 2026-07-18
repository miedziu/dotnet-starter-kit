using Mediator;

namespace FSH.Mod.Identity.Spec.v1.Group;

public sealed record DeleteGroupCommand(Guid Id) : ICommand<Unit>;