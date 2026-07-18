using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Mod.Billing.Data;
using FSH.Mod.Billing.Domain;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Billing.Features.v1.Plans;

public static class CreatePlanEndpoint
{
    internal static RouteHandlerBuilder MapCreatePlanEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/plans",
                async (CreatePlanCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateBillingPlan")
            .WithSummary("Create a new billing plan")
            .RequirePermission(BillingPermissions.Manage)
            .WithIdempotency();
    }
}

public sealed class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.MonthlyBasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Interval).IsInEnum();
        RuleFor(x => x.AnnualPrice).GreaterThanOrEqualTo(0).When(x => x.AnnualPrice.HasValue);
    }
}

public sealed class CreatePlanCommandHandler(BillingDbContext dbContext)
    : ICommandHandler<CreatePlanCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreatePlanCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var plan = BillingPlan.Create(command.Key, command.Name, command.Currency, command.MonthlyBasePrice, command.Interval, command.AnnualPrice);
        dbContext.Plans.Add(plan);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return plan.Id;
    }
}