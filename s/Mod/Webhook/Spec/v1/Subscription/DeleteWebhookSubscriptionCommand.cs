using Mediator;

namespace FSH.Mod.Webhook.Spec.v1.Subscription;

public sealed record DeleteWebhookSubscriptionCommand(Guid Id) : ICommand;