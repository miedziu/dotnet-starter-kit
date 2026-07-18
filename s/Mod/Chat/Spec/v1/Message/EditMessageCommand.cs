using Mediator;

namespace FSH.Mod.Chat.Spec.v1.Message;

public sealed record EditMessageCommand(Guid MessageId, string Body) : ICommand<Unit>;