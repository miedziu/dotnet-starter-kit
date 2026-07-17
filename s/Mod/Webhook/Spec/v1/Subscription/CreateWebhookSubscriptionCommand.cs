using Mediator;

namespace FSH.Modules.Webhooks.Contracts.v1.Subscription;

public sealed record CreateWebhookSubscriptionCommand(
    string Url,
    string[] Events,
    string? Secret) : ICommand<Guid>;