namespace FSH.Mods.Chat.Spec.v1;

public sealed record ChannelDto(
    Guid Id,
    ChannelType Type,
    string? Name,
    string? Slug,
    string? Description,
    bool IsPrivate,
    string CreatedByUserId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? LastMessageAtUtc,
    int UnreadCount,
    IReadOnlyList<ChannelMemberDto> Members);