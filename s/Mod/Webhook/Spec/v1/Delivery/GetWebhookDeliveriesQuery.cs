using FSH.Framework.Shared.Persistence;
using FSH.Modules.Webhooks.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Webhooks.Contracts.v1.Deliveries;

public sealed record GetWebhookDeliveriesQuery(Guid SubscriptionId, int PageNumber = 1, int PageSize = 10)
    : IQuery<PagedResponse<WebhookDeliveryDto>>;