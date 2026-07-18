namespace FSH.Mod.Chat.Spec.v1;

/// <summary>
/// Channel kind. Serialized as its string name (global JsonStringEnumConverter), so the SPA
/// sees "DirectMessage"/"GroupMessage"/"Channel". Lives in Spec because it is part of the
/// published wire contract (ChannelDto).
/// </summary>
public enum ChannelType
{
    DirectMessage = 0,
    GroupMessage = 1,
    Channel = 2,
}