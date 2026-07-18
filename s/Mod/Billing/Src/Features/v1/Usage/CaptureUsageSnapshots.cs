using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Mod.Billing.Services;
using FSH.Mod.Billing.Spec.v1.Usage;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Billing.Features.v1.Usage;

public static class CaptureUsageSnapshotsEndpoint
{
    internal static RouteHandlerBuilder MapCaptureUsageSnapshotsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/usage/snapshots/capture",
                async (CaptureUsageSnapshotsCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CaptureUsageSnapshots")
            .WithSummary("Manually capture usage snapshots for a tenant + period")
            .WithDescription("Ops endpoint wrapping IUsageReporter.CaptureForPeriodAsync. Idempotent: re-running for the same (tenant, period) returns existing snapshots unchanged. Used for retroactive billing, debugging, and re-runs after fixes.")
            .RequirePermission(BillingPermissions.Manage)
            .WithIdempotency()
            .Produces<IReadOnlyList<UsageSnapshotDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}

public sealed class CaptureUsageSnapshotsCommandValidator : AbstractValidator<CaptureUsageSnapshotsCommand>
{
    public CaptureUsageSnapshotsCommandValidator()
    {
        RuleFor(x => x.PeriodYear).InclusiveBetween(2000, 2100);
        RuleFor(x => x.PeriodMonth).InclusiveBetween(1, 12);
    }
}

public sealed class CaptureUsageSnapshotsCommandHandler(
    IUsageReporter reporter)
    : ICommandHandler<CaptureUsageSnapshotsCommand, IReadOnlyList<UsageSnapshotDto>>
{
    public async ValueTask<IReadOnlyList<UsageSnapshotDto>> Handle(
        CaptureUsageSnapshotsCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var snapshots = await reporter
            .CaptureForPeriodAsync(command.PeriodYear, command.PeriodMonth, cancellationToken)
            .ConfigureAwait(false);

        return snapshots
            .Select(s => new UsageSnapshotDto(
                s.Id,
                s.PeriodYear,
                s.PeriodMonth,
                s.UsedUnits,
                s.LimitUnits,
                s.Overage,
                s.CapturedAtUtc))
            .ToList();
    }
}