using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Mod.Billing.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Mod.Billing.Features.v1.Invoices;

public static class GenerateInvoicesEndpoint
{
    internal static RouteHandlerBuilder MapGenerateInvoicesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/invoices/generate",
                async (GenerateInvoicesCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(new { generated = await mediator.Send(command, ct) }))
            .WithName("GenerateInvoices")
            .WithSummary("Manually trigger invoice generation for a period")
            .RequirePermission(BillingPermissions.Manage)
            .WithIdempotency();
    }
}

public sealed class GenerateInvoicesCommandValidator : AbstractValidator<GenerateInvoicesCommand>
{
    public GenerateInvoicesCommandValidator()
    {
        RuleFor(x => x.PeriodYear).InclusiveBetween(2000, 2100);
        RuleFor(x => x.PeriodMonth).InclusiveBetween(1, 12);
    }
}

public sealed class GenerateInvoicesCommandHandler(
    IBillingService billing)
    : ICommandHandler<GenerateInvoicesCommand, int>
{
    public async ValueTask<int> Handle(GenerateInvoicesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return await billing.GenerateInvoicesAsync(command.PeriodYear, command.PeriodMonth, cancellationToken).ConfigureAwait(false);
    }
}