using FluentValidation;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Billing.Contracts.Authorization;
using FSH.Modules.Billing.Contracts.v1.Plans;
using FSH.Modules.Billing.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Billing.Features.v1.Plans;

public static class UpdatePlanEndpoint
{
    internal static RouteHandlerBuilder MapUpdatePlanEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/plans/{planId:guid}",
                async (Guid planId, UpdatePlanCommand body, IMediator mediator, CancellationToken ct) =>
                {
                    ArgumentNullException.ThrowIfNull(body);
                    var command = body with { PlanId = planId };
                    return Results.Ok(await mediator.Send(command, ct));
                })
            .WithName("UpdateBillingPlan")
            .WithSummary("Update a billing plan")
            .RequirePermission(BillingPermissions.Manage);
    }
}

public sealed class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator()
    {
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.MonthlyBasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Interval).IsInEnum();
        RuleFor(x => x.AnnualPrice).GreaterThanOrEqualTo(0).When(x => x.AnnualPrice.HasValue);
    }
}

public sealed class UpdatePlanCommandHandler(BillingDbContext dbContext)
    : ICommandHandler<UpdatePlanCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdatePlanCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var plan = await dbContext.Plans.FirstOrDefaultAsync(p => p.Id == command.PlanId, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Plan {command.PlanId} not found.");

        plan.Update(command.Name, command.MonthlyBasePrice, command.Interval, command.AnnualPrice);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return plan.Id;
    }
}