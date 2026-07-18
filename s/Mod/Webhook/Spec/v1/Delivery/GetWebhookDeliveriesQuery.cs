using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Mod.Webhook.Spec.v1.Delivery;

public sealed record GetWebhookDeliveriesQuery(Guid SubscriptionId, int PageNumber = 1, int PageSize = 10)
    : IQuery<PagedResponse<WebhookDeliveryDto>>;