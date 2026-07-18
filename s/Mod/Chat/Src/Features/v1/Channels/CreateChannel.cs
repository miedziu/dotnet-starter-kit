using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Chat.Data;
using FSH.Mod.Chat.Domain;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Chat.Features.v1.Channels;

public static class CreateChannelEndpoint
{
    internal static RouteHandlerBuilder MapCreateChannelEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/channels",
                async (CreateChannelCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateChannel")
            .WithSummary("Create a new named chat channel")
            .RequirePermission(ChatPermissions.Channels.Create);
}

public sealed class CreateChannelCommandValidator : AbstractValidator<CreateChannelCommand>
{
    public CreateChannelCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
    }
}

public sealed class CreateChannelCommandHandler(
    ChatDbContext db,
    ICurrentUser currentUser)
    : ICommandHandler<CreateChannelCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateChannelCommand cmd, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        var userId = currentUser.GetUserId().ToString();
        if (userId == Guid.Empty.ToString())
        {
            throw new UnauthorizedException("no current user");
        }

        var channel = ChatChannel.CreateChannel(cmd.Name, cmd.Description, cmd.IsPrivate, userId);
        db.Channels.Add(channel);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return channel.Id;
    }
}