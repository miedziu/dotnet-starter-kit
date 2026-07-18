using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Mod.Ticket.Data;
using FSH.Mod.Ticket.Spec.v1.Ticket;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Ticket.Features.v1;

public static class ReopenTicketEndpoint
{
    internal static RouteHandlerBuilder MapReopenTicketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/tickets/{ticketId:guid}/reopen",
                async (Guid ticketId, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new ReopenTicketCommand(ticketId), ct)))
            .WithName("ReopenTicket")
            .WithSummary("Reopen a resolved or closed ticket")
            .RequirePermission(TicketPermissions.Ticket.Reopen)
            .WithIdempotency();
    }
}

public sealed class ReopenTicketCommandHandler(TicketDbContext dbContext)
    : ICommandHandler<ReopenTicketCommand, Guid>
{
    public async ValueTask<Guid> Handle(ReopenTicketCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ticket = await dbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Ticket {command.TicketId} not found.");

        ticket.Reopen();
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ticket.Id;
    }
}