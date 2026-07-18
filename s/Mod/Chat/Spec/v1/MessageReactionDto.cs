namespace FSH.Mod.Chat.Spec.v1;

public sealed record MessageReactionDto(
    Guid Id,
    Guid MessageId,
    string UserId,
    string Emoji,
    DateTime CreatedAtUtc);