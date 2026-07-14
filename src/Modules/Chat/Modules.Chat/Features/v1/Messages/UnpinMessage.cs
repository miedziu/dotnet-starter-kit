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
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Chat.Features.v1.Messages;

public static class UnpinMessageEndpoint
{
    internal static RouteHandlerBuilder MapUnpinMessageEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapDelete("/messages/{id:guid}/pin",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new UnpinMessageCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("UnpinMessage")
            .WithSummary("Remove a pin from a message")
            .RequirePermission(ChatPermissions.Messages.Send);
}

public sealed class UnpinMessageCommandValidator : AbstractValidator<UnpinMessageCommand>
{
    public UnpinMessageCommandValidator()
    {
        RuleFor(x => x.MessageId).NotEmpty();
    }
}

public sealed class UnpinMessageCommandHandler(
    ChatDbContext db,
    ICurrentUser currentUser,
    IHubContext<AppHub> hub)
    : ICommandHandler<UnpinMessageCommand, Unit>
{
    public async ValueTask<Unit> Handle(UnpinMessageCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");
        var currentUserId = userId.ToString();

        var message = await db.Messages.FirstOrDefaultAsync(m => m.Id == cmd.MessageId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Message not found.");

        var channel = await db.Channels.FirstOrDefaultAsync(c => c.Id == message.ChannelId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Message not found.");
        channel.RequireMember(currentUserId);

        message.Unpin(currentUserId);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await hub.Clients.Group($"channel:{channel.Id}")
            .SendAsync("ChatMessageUnpinned", message.ToDto(), cancellationToken)
            .ConfigureAwait(false);
        return Unit.Value;
    }
}