using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Mods.Webhook.Spec.v1.Subscription;

public sealed record GetWebhookSubscriptionsQuery(int PageNumber = 1, int PageSize = 10)
    : IQuery<PagedResponse<WebhookSubscriptionDto>>;