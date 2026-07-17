using Mediator;

namespace FSH.Modules.Webhooks.Contracts.v1.Subscription;

public sealed record TestWebhookSubscriptionCommand(Guid Id) : ICommand<bool>;