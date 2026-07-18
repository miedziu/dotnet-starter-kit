using Mediator;

namespace FSH.Mods.Webhook.Spec.v1.Subscription;

public sealed record CreateWebhookSubscriptionCommand(
    string Url,
    string[] Events,
    string? Secret) : ICommand<Guid>;