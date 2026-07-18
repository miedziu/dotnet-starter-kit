using Mediator;

namespace FSH.Mods.Chat.Spec.v1.Channel;

/// <summary>
/// Create a named channel. Creator becomes the first member with Admin role. Use <see cref="FindOrCreateDmCommand"/>
/// for DMs / group DMs instead.
/// </summary>
public sealed record CreateChannelCommand(
    string Name,
    string? Description,
    bool IsPrivate) : ICommand<Guid>;