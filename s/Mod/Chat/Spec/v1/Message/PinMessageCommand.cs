using Mediator;

namespace FSH.Mod.Chat.Spec.v1.Message;

public sealed record PinMessageCommand(Guid MessageId) : ICommand<Unit>;

public sealed record UnpinMessageCommand(Guid MessageId) : ICommand<Unit>;