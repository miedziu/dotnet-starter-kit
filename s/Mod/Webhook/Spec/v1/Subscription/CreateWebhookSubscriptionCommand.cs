using Mediator;

namespace FSH.Mod.Webhook.Spec.v1.Subscription;

public sealed record CreateWebhookSubscriptionCommand(
    string Url,
    string[] Events,
    string? Secret) : ICommand<Guid>;