using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Billing.Contracts.Authorization;
using FSH.Modules.Billing.Contracts.v1.Invoices;
using FSH.Modules.Billing.Services;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Billing.Features.v1.Invoices;

public static class VoidInvoiceEndpoint
{
    public sealed record VoidInvoiceBody(string? Reason);

    internal static RouteHandlerBuilder MapVoidInvoiceEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/invoices/{invoiceId:guid}/void",
                async (Guid invoiceId, VoidInvoiceBody? body, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new VoidInvoiceCommand(invoiceId, body?.Reason), ct)))
            .WithName("VoidInvoice")
            .WithSummary("Void an invoice")
            .RequirePermission(BillingPermissions.Manage);
    }
}

public sealed class VoidInvoiceCommandHandler(IBillingService billing)
    : ICommandHandler<VoidInvoiceCommand, Guid>
{
    public async ValueTask<Guid> Handle(VoidInvoiceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        await billing.VoidInvoiceAsync(command.InvoiceId, command.Reason, cancellationToken).ConfigureAwait(false);
        return command.InvoiceId;
    }
}