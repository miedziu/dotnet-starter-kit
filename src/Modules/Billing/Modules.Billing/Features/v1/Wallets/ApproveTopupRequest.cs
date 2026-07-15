using FluentValidation;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Billing.Contracts.Authorization;
using FSH.Modules.Billing.Contracts.v1.Wallets;
using FSH.Modules.Billing.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Billing.Features.v1.Wallets;

public static class ApproveTopupRequestEndpoint
{
    public sealed record ApproveTopupRequestBody(string? Note);

    internal static RouteHandlerBuilder MapApproveTopupRequestEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/wallet/topup-requests/{id:guid}/approve",
                async (Guid id, ApproveTopupRequestBody? body, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new ApproveTopupRequestCommand(id, body?.Note), ct)))
            .WithName("ApproveTopupRequest")
            .WithSummary("Approve a pending top-up request and issue the invoice")
            .RequirePermission(BillingPermissions.Manage)
            .WithIdempotency();
    }
}
public sealed class ApproveTopupRequestCommandValidator : AbstractValidator<ApproveTopupRequestCommand>
{
    public ApproveTopupRequestCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class ApproveTopupRequestCommandHandler(
    IBillingService billing)
    : ICommandHandler<ApproveTopupRequestCommand, Guid>
{
    public async ValueTask<Guid> Handle(ApproveTopupRequestCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // For root, operate on the request's own tenant; for non-root, callerTenantId equals request.TenantId.
        var invoice = await billing.CreateTopupInvoiceAsync(command.Id, cancellationToken)
            .ConfigureAwait(false);

        return invoice.Id;
    }
}