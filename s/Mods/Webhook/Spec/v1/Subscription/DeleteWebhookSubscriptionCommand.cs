using Mediator;

namespace FSH.Mods.Webhook.Spec.v1.Subscription;

public sealed record DeleteWebhookSubscriptionCommand(Guid Id) : ICommand;