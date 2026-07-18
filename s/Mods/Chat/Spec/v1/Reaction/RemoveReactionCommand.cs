using Mediator;

namespace FSH.Mods.Chat.Spec.v1.Reaction;

public sealed record RemoveReactionCommand(Guid MessageId, string Emoji) : ICommand<Unit>;