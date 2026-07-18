using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mods.Webhook.Data;
using FSH.Mods.Webhook.Services;
using FSH.Mods.Webhook.Spec;
using FSH.Mods.Webhook.Spec.v1.Subscription;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FSH.Mods.Webhook.Features.v1;

public static class TestWebhookSubscriptionEndpoint
{
    internal static RouteHandlerBuilder MapTestWebhookSubscriptionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/subscriptions/{id:guid}/test", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var success = await mediator.Send(new TestWebhookSubscriptionCommand(id), ct);
            return TypedResults.Ok(new { Success = success });
        })
        .WithName("TestWebhookSubscription")
        .WithSummary("Send a test event to a webhook subscription")
        .RequirePermission(WebhookPermissions.Subscriptions.Test);
    }
}

public sealed class TestWebhookSubscriptionCommandHandler(
    WebhookDbContext dbContext,
    IWebhookDeliveryService deliveryService,
    IWebhookSecretProtector secretProtector) : ICommandHandler<TestWebhookSubscriptionCommand, bool>
{
    public async ValueTask<bool> Handle(TestWebhookSubscriptionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var subscription = await dbContext.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Webhook subscription {command.Id} not found.");

        var testPayload = JsonSerializer.Serialize(new
        {
            eventType = "webhook.test",
            timestamp = TimeProvider.System.GetUtcNow().UtcDateTime,
            message = "This is a test webhook delivery."
        });

        await deliveryService.DeliverAsync(
            subscription.Id,
            subscription.Url,
            secretProtector.Unprotect(subscription.ProtectedSecret),
            "webhook.test",
            testPayload,
            cancellationToken).ConfigureAwait(false);

        return true;
    }
}