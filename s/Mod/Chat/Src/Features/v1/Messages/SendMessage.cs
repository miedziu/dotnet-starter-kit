using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Eventing.Abstractions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Framework.Web.Realtime;
using FSH.Mod.Chat.Data;
using FSH.Mod.Chat.Domain;
using FSH.Mod.Chat.Features.v1.Internal;
using FSH.Mod.Chat.Services;
using FSH.Mod.Chat.Spec.Events;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;

namespace FSH.Mod.Chat.Features.v1.Messages;

public static class SendMessageEndpoint
{
    internal static RouteHandlerBuilder MapSendMessageEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/channels/{id:guid}/messages",
                async (Guid id, [FromBody] SendMessageBody body, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(
                        new SendMessageCommand(id, body.Body, body.ParentMessageId, body.Attachments ?? []),
                        ct)))
            .WithName("SendMessage")
            .WithSummary("Send a message to a channel — supports replies (parentMessageId) and attachments")
            .RequirePermission(ChatPermissions.Messages.Send)
            .WithIdempotency();

    public sealed record SendMessageBody(
        string? Body,
        Guid? ParentMessageId,
        IReadOnlyList<SendMessageAttachmentInput>? Attachments);
}

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty();
        // Body is optional when an attachment is present (Slack/Teams parity — "here's the file" with
        // no text). Length cap still applies whenever body is populated.
        RuleFor(x => x.Body)
            .NotEmpty()
            .When(x => x.Attachments is null || x.Attachments.Count == 0)
            .WithMessage("Either a body or an attachment is required.");
        RuleFor(x => x.Body)
            .MaximumLength(32_768)
            .When(x => !string.IsNullOrEmpty(x.Body));
        RuleFor(x => x.Attachments).NotNull();
        RuleFor(x => x.Attachments.Count).LessThanOrEqualTo(10)
            .When(x => x.Attachments is not null);
        RuleForEach(x => x.Attachments).ChildRules(att =>
        {
            att.RuleFor(a => a.Url).NotEmpty().MaximumLength(2048);
            att.RuleFor(a => a.ContentType).NotEmpty().MaximumLength(255);
            att.RuleFor(a => a.FileName).NotEmpty().MaximumLength(512);
            att.RuleFor(a => a.SizeBytes).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class SendMessageCommandHandler(
    ChatDbContext db,
    ICurrentUser currentUser,
    IHubContext<AppHub> hub,
    IMentionResolver mentionResolver,
    IEventBus eventBus)
    : ICommandHandler<SendMessageCommand, MessageDto>
{
    public async ValueTask<MessageDto> Handle(SendMessageCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        var channel = await db.Channels.FirstOrDefaultAsync(c => c.Id == cmd.ChannelId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Channel not found.");

        channel.RequireMember(currentUserId);

        Message? parent = null;
        if (cmd.ParentMessageId is { } parentId)
        {
            parent = await db.Messages.FirstOrDefaultAsync(m => m.Id == parentId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException("Parent message not found.");
            if (parent.ChannelId != channel.Id)
            {
                throw new CustomException("Parent message belongs to a different channel.", (IEnumerable<string>?)null, HttpStatusCode.BadRequest);
            }
            if (parent.ParentMessageId.HasValue)
            {
                // 1-level deep only per spec.
                throw new CustomException("Cannot reply to a reply — threads are single-level only.", (IEnumerable<string>?)null, HttpStatusCode.BadRequest);
            }
        }

        // Parse @username tokens to user ids; self-mentions and unresolved tokens are dropped silently.
        // Only resolved *other* users attach as MessageMention rows + fire an event. Body may be empty.
        var rawMatches = MentionParser.Parse(cmd.Body ?? string.Empty);
        var distinctNames = rawMatches.Select(m => m.Username)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var resolved = distinctNames.Length == 0
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : (Dictionary<string, string>)await mentionResolver
                .ResolveUserIdsAsync(distinctNames, cancellationToken)
                .ConfigureAwait(false);

        var parsedMentions = new List<Message.ParsedMention>();
        var notifyUserIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var match in rawMatches)
        {
            if (!resolved.TryGetValue(match.Username, out var mentionedUserId)) continue;
            if (string.Equals(mentionedUserId, currentUserId, StringComparison.Ordinal)) continue;
            parsedMentions.Add(new Message.ParsedMention(mentionedUserId, match.StartIndex, match.Length));
            notifyUserIds.Add(mentionedUserId);
        }

        var message = Message.Create(channel.Id, currentUserId, cmd.Body, parent?.Id, parsedMentions);
        foreach (var att in cmd.Attachments ?? Array.Empty<SendMessageAttachmentInput>())
        {
            message.AddAttachment(att.FileAssetId, att.Url, att.ContentType, att.FileName, att.SizeBytes);
        }

        db.Messages.Add(message);

        if (parent is not null)
        {
            parent.IncrementReplyCount();
        }
        channel.TouchLastMessage(DateTime.UtcNow);

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var dto = message.ToDto();
        await hub.Clients.Group($"channel:{channel.Id}")
            .SendAsync("ChatMessageCreated", dto, cancellationToken)
            .ConfigureAwait(false);

        // One integration event per distinct mentioned user. Notification module subscribes.
        if (notifyUserIds.Count > 0)
        {
            var correlationId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
            var preview = MakePreview(message.Body ?? string.Empty);
            foreach (var mentionedUserId in notifyUserIds)
            {
                await eventBus.PublishAsync(
                    new MentionedInChannelIntegrationEvent(
                        Id: Guid.NewGuid(),
                        OccurredAt: DateTime.UtcNow,
                        CorrelationId: correlationId,
                        Source: "Chat",
                        ChannelId: channel.Id,
                        ChannelName: channel.Name,
                        MessageId: message.Id,
                        AuthorUserId: currentUserId,
                        MentionedUserId: mentionedUserId,
                        BodyPreview: preview),
                    cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return dto;
    }

    /// <summary>
    /// Truncate the body for inbox display. Keeps things to a single line,
    /// at most 140 characters.
    /// </summary>
    private static string MakePreview(string body)
    {
        var collapsed = body.Replace('\r', ' ').Replace('\n', ' ').Trim();
        const int max = 140;
        if (collapsed.Length <= max) return collapsed;
        return string.Concat(collapsed.AsSpan(0, max - 1), "…"); // … ellipsis
    }
}