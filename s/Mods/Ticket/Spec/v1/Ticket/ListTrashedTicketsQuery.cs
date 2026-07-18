using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Mods.Ticket.Spec.v1.Ticket;

public sealed record ListTrashedTicketsQuery(int PageNumber = 1, int PageSize = 20)
    : IQuery<PagedResponse<TicketDto>>;