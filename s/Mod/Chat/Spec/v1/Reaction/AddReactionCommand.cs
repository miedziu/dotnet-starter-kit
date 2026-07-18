using Mediator;

namespace FSH.Mod.Chat.Spec.v1.Reaction;

public sealed record AddReactionCommand(Guid MessageId, string Emoji) : ICommand<Unit>;