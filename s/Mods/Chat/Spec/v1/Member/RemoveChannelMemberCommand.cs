using Mediator;

namespace FSH.Mods.Chat.Spec.v1.Member;

public sealed record RemoveChannelMemberCommand(
    Guid ChannelId,
    string UserId) : ICommand<Unit>;