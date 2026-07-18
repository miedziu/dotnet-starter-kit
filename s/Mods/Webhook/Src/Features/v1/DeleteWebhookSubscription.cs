using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.Webhook.Data;
using FSH.Mods.Webhook.Spec;
using FSH.Mods.Webhook.Spec.v1.Subscription;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.Webhook.Features.v1;

public static class DeleteWebhookSubscriptionEndpoint
{
    internal static RouteHandlerBuilder MapDeleteWebhookSubscriptionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/subscriptions/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            await mediator.Send(new DeleteWebhookSubscriptionCommand(id), ct);
            return TypedResults.NoContent();
        })
        .WithName("DeleteWebhookSubscription")
        .WithSummary("Delete a webhook subscription")
        .RequirePermission(WebhookPermissions.Subscriptions.Delete)
        .Produces(StatusCodes.Status204NoContent);
    }
}

public sealed class DeleteWebhookSubscriptionCommandHandler(
    WebhookDbContext dbContext) : ICommandHandler<DeleteWebhookSubscriptionCommand>
{
    public async ValueTask<Unit> Handle(DeleteWebhookSubscriptionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var subscription = await dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Webhook subscription {command.Id} not found.");

        dbContext.Subscriptions.Remove(subscription);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Unit.Value;
    }
}