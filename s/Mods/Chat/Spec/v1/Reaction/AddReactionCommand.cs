using Mediator;

namespace FSH.Mods.Chat.Spec.v1.Reaction;

public sealed record AddReactionCommand(Guid MessageId, string Emoji) : ICommand<Unit>;