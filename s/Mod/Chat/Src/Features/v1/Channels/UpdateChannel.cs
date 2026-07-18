using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Chat.Data;
using FSH.Mod.Chat.Features.v1.Internal;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Chat.Features.v1.Channels;

public static class UpdateChannelEndpoint
{
    internal static RouteHandlerBuilder MapUpdateChannelEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPut("/channels/{id:guid}",
                async (Guid id, [FromBody] UpdateChannelBody body, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new UpdateChannelCommand(id, body.Name, body.Description, body.IsPrivate), ct);
                    return Results.NoContent();
                })
            .WithName("UpdateChannel")
            .WithSummary("Rename / re-describe / re-privacy a named channel (channel admin only)")
            .RequirePermission(ChatPermissions.Channels.Create);

    public sealed record UpdateChannelBody(string Name, string? Description, bool IsPrivate);
}

public sealed class UpdateChannelCommandValidator : AbstractValidator<UpdateChannelCommand>
{
    public UpdateChannelCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
    }
}

public sealed class UpdateChannelCommandHandler(
    ChatDbContext db,
    ICurrentUser currentUser)
    : ICommandHandler<UpdateChannelCommand, Unit>
{
    public async ValueTask<Unit> Handle(UpdateChannelCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var userId = currentUser.GetUserId();
        if (userId == Guid.Empty) throw new UnauthorizedException("no current user");

        var channel = await db.Channels.FirstOrDefaultAsync(c => c.Id == cmd.ChannelId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException("Channel not found.");

        channel.RequireAdmin(userId.ToString());
        channel.Rename(cmd.Name, cmd.Description);
        channel.SetPrivate(cmd.IsPrivate);

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}