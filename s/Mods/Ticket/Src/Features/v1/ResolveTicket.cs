using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Mods.Ticket.Data;
using FSH.Mods.Ticket.Spec;
using FSH.Mods.Ticket.Spec.v1.Ticket;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mods.Ticket.Features.v1;

public static class ResolveTicketEndpoint
{
    public sealed record ResolveTicketRequest(string? ResolutionNote);

    internal static RouteHandlerBuilder MapResolveTicketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/tickets/{ticketId:guid}/resolve",
                async (Guid ticketId, ResolveTicketRequest? body, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new ResolveTicketCommand(ticketId, body?.ResolutionNote), ct)))
            .WithName("ResolveTicket")
            .WithSummary("Mark a ticket as resolved")
            .RequirePermission(TicketPermissions.Ticket.Resolve)
            .WithIdempotency();
    }
}

public sealed class ResolveTicketCommandHandler(TicketDbContext dbContext)
    : ICommandHandler<ResolveTicketCommand, Guid>
{
    public async ValueTask<Guid> Handle(ResolveTicketCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ticket = await dbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Ticket {command.TicketId} not found.");

        ticket.Resolve(command.ResolutionNote);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ticket.Id;
    }
}