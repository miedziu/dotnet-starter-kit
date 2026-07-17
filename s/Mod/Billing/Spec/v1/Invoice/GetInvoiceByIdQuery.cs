using FSH.Modules.Billing.Contracts.v1.Dtos;
using Mediator;

namespace FSH.Modules.Billing.Contracts.v1.Invoices;

public sealed record GetInvoiceByIdQuery(Guid InvoiceId) : IQuery<InvoiceDto>;