using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Realtime;
using FSH.Modules.Chat.Contracts.Authorization;
using FSH.Modules.Chat.Contracts.v1.Commands;
using FSH.Modules.Chat.Data;
using FSH.Modules.Chat.Features.v1.Internal;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Chat.Features.v1.Messages;

public static class EditMessageEndpoint
{
    internal static RouteHandlerBuilder MapEditMessageEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPut("/messages/{id:guid}",
                async (Guid id, [FromBody] EditMessageBody body, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new EditMessageCommand(id, body.Body), ct);
                    return Results.NoContent();
                })
            .WithName("UpdateMessage")
            .WithSummary("Edit a message — author only")
            .RequirePermission(ChatPermissions.Messages.EditOwn);

    public sealed record EditMessageBody(string Body);
}

public sealed class EditMessageCommandValidator : AbstractValidator<EditMessageCommand>
{
    public EditMessageCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(32_768);
    }
}

public sealed class EditMessageCommandHandler(
    ChatDbContext db,
    ICurrentUser currentUser,
    IHubContext<AppHub> hub)
    : ICommandHandler<EditMessageCommand, Unit>
{
    public async ValueTask<Unit> Handle(EditMessageCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        var message = await db.Messages.FirstOrDefaultAsync(m => m.Id == cmd.MessageId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Message not found.");

        // Verify membership through the parent channel (don't leak existence to non-members).
        var channel = await db.Channels.FirstOrDefaultAsync(c => c.Id == message.ChannelId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Message not found.");
        channel.RequireMember(currentUserId);

        message.Edit(cmd.Body, currentUserId); // domain enforces author-only
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await hub.Clients.Group($"channel:{channel.Id}")
            .SendAsync("ChatMessageEdited", message.ToDto(), cancellationToken)
            .ConfigureAwait(false);
        return Unit.Value;
    }
}