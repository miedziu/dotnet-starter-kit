namespace FSH.Mod.Chat.Spec.v1;

/// <summary>
/// Member role within a channel. Serialized as its string name (global JsonStringEnumConverter),
/// so the SPA sees "Member"/"Admin". Lives in Spec because it is part of the published wire
/// contract (ChannelMemberDto).
/// </summary>
public enum ChannelMemberRole
{
    Member = 0,
    Admin = 1,
}