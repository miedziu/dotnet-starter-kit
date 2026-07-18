using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Mod.Webhook.Data;
using FSH.Mod.Webhook.Domain;
using FSH.Mod.Webhook.Services;
using FSH.Mod.Webhook.Spec.v1.Subscription;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Webhook.Features.v1;

public static class CreateWebhookSubscriptionEndpoint
{
    internal static RouteHandlerBuilder MapCreateWebhookSubscriptionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/subscriptions", async (
            CreateWebhookSubscriptionCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var id = await mediator.Send(command, ct);
            return TypedResults.Created($"/api/v1/webhooks/subscriptions/{id}", id);
        })
        .WithName("CreateWebhookSubscription")
        .WithSummary("Create a webhook subscription")
        .RequirePermission(WebhookPermissions.Subscriptions.Create)
        .WithIdempotency()
        .Produces<Guid>(StatusCodes.Status201Created);
    }
}

public sealed class CreateWebhookSubscriptionCommandHandler(
    WebhookDbContext dbContext,
    IWebhookSecretProtector secretProtector) : ICommandHandler<CreateWebhookSubscriptionCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateWebhookSubscriptionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Encrypt the signing secret at rest — it is the HMAC key, so it must be recoverable
        // (not hashed). Decrypted only at dispatch time to sign the outbound payload.
        var protectedSecret = secretProtector.Protect(command.Secret);
        var subscription = WebhookSubscription.Create(command.Url, command.Events, protectedSecret);
        dbContext.Subscriptions.Add(subscription);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return subscription.Id;
    }
}