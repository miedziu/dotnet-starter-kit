namespace FSH.Mods.Chat.Spec.v1;

public sealed record ChannelMemberDto(
    Guid Id,
    string UserId,
    ChannelMemberRole Role,
    DateTime JoinedAtUtc,
    Guid? LastReadMessageId);