using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Billing.Contracts.Authorization;
using FSH.Modules.Billing.Contracts.v1.Invoices;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using FSH.Modules.Billing.Services;

namespace FSH.Modules.Billing.Features.v1.Invoices;

public static class IssueInvoiceEndpoint
{
    public sealed record IssueInvoiceBody(DateTime? DueAtUtc);

    internal static RouteHandlerBuilder MapIssueInvoiceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/invoices/{invoiceId:guid}/issue",
                async (Guid invoiceId, IssueInvoiceBody? body, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new IssueInvoiceCommand(invoiceId, body?.DueAtUtc), ct)))
            .WithName("IssueInvoice")
            .WithSummary("Issue a draft invoice")
            .RequirePermission(BillingPermissions.Manage)
            .WithIdempotency();
    }
}

public sealed class IssueInvoiceCommandHandler(IBillingService billing)
    : ICommandHandler<IssueInvoiceCommand, Guid>
{
    public async ValueTask<Guid> Handle(IssueInvoiceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await billing.IssueInvoiceAsync(command.InvoiceId, command.DueAtUtc, cancellationToken).ConfigureAwait(false);
        return command.InvoiceId;
    }
}