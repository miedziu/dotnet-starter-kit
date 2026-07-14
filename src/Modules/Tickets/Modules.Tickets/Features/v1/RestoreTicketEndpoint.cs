using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Tickets.Contracts.Authorization;
using FSH.Modules.Tickets.Contracts.v1.Tickets;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using FSH.Modules.Tickets.Data;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Tickets.Features.v1;

public static class RestoreTicketEndpoint
{
    internal static RouteHandlerBuilder MapRestoreTicketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/tickets/{ticketId:guid}/restore",
                async (Guid ticketId, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new RestoreTicketCommand(ticketId), ct)))
            .WithName("RestoreTicket")
            .WithSummary("Restore a soft-deleted ticket")
            .RequirePermission(TicketsPermissions.Tickets.Restore)
            .WithIdempotency();
    }
}

public sealed class RestoreTicketCommandHandler(TicketsDbContext dbContext)
    : ICommandHandler<RestoreTicketCommand, Guid>
{
    public async ValueTask<Guid> Handle(RestoreTicketCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ticket = await dbContext.Tickets
            .IgnoreQueryFilters([QueryFilters.SoftDelete])
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Ticket {command.TicketId} not found.");

        ticket.Restore();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ticket.Id;
    }
}