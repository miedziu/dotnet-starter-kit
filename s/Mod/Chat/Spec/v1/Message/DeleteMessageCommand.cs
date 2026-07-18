using Mediator;

namespace FSH.Mod.Chat.Spec.v1.Message;

public sealed record DeleteMessageCommand(Guid MessageId) : ICommand<Unit>;