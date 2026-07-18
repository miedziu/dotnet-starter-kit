using Mediator;

namespace FSH.Mod.Webhook.Spec.v1.Subscription;

public sealed record TestWebhookSubscriptionCommand(Guid Id) : ICommand<bool>;