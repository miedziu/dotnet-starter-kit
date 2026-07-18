using FluentValidation;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Mod.Billing.Data;
using FSH.Mod.Billing.Domain;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Billing.Features.v1.Subscriptions;

public static class AssignSubscriptionEndpoint
{
    internal static RouteHandlerBuilder MapAssignSubscriptionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/subscriptions",
                async (AssignSubscriptionCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("AssignSubscription")
            .WithSummary("Assign a plan to the application")
            .RequirePermission(BillingPermissions.Manage)
            .WithIdempotency();
    }
}

public sealed class AssignSubscriptionCommandValidator : AbstractValidator<AssignSubscriptionCommand>
{
    public AssignSubscriptionCommandValidator()
    {
        RuleFor(x => x.PlanKey).NotEmpty().MaximumLength(64);
    }
}

public sealed class AssignSubscriptionCommandHandler(
    BillingDbContext dbContext)
    : ICommandHandler<AssignSubscriptionCommand, Guid>
{
    public async ValueTask<Guid> Handle(AssignSubscriptionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
#pragma warning disable CA1308 // Plan keys are canonical lowercase slugs
        var key = command.PlanKey.ToLowerInvariant();
#pragma warning restore CA1308
        var plan = await dbContext.Plans.FirstOrDefaultAsync(p => p.Key == key && p.IsActive, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Active plan with key '{command.PlanKey}' not found.");

        var now = DateTime.UtcNow;
        var current = await dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.Status == Contracts.SubscriptionStatus.Active, cancellationToken)
            .ConfigureAwait(false);
        current?.Cancel(now);

        var subscription = Subscription.Create(plan.Id, now);
        dbContext.Subscriptions.Add(subscription);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return subscription.Id;
    }
}