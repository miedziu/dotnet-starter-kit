using Mediator;

namespace FSH.Mod.Chat.Spec.v1.Reaction;

public sealed record RemoveReactionCommand(Guid MessageId, string Emoji) : ICommand<Unit>;