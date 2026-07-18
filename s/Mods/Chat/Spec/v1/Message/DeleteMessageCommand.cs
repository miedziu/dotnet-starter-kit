using Mediator;

namespace FSH.Mods.Chat.Spec.v1.Message;

public sealed record DeleteMessageCommand(Guid MessageId) : ICommand<Unit>;