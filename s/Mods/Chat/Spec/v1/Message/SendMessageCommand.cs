using Mediator;

namespace FSH.Mods.Chat.Spec.v1.Message;

public sealed record SendMessageAttachmentInput(
    Guid? FileAssetId,
    string Url,
    string ContentType,
    string FileName,
    long SizeBytes);

public sealed record SendMessageCommand(
    Guid ChannelId,
    string? Body,
    Guid? ParentMessageId,
    IReadOnlyList<SendMessageAttachmentInput> Attachments) : ICommand<MessageDto>;