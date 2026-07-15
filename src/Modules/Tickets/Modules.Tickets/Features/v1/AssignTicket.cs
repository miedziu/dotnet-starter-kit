using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Tickets.Contracts.Authorization;
using FSH.Modules.Tickets.Contracts.v1.Tickets;
using FSH.Modules.Tickets.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Tickets.Features.v1;

public static class AssignTicketEndpoint
{
    internal static RouteHandlerBuilder MapAssignTicketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/tickets/{ticketId:guid}/assign",
                async (Guid ticketId, Guid? assigneeUserId, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new AssignTicketCommand(ticketId, assigneeUserId), ct)))
            .WithName("AssignTicket")
            .WithSummary("Assign or reassign a ticket")
            .RequirePermission(TicketsPermissions.Tickets.Assign)
            .WithIdempotency();
    }
}

public sealed class AssignTicketCommandHandler(
    TicketsDbContext dbContext,
    IUserProfileService userProfileService)
    : ICommandHandler<AssignTicketCommand, Guid>
{
    public async ValueTask<Guid> Handle(AssignTicketCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ticket = await dbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Ticket {command.TicketId} not found.");

        int? assigneeUserId = null;
        if (command.AssigneeUserId.HasValue)
        {
            assigneeUserId = await userProfileService.GetIntIdAsync(
                command.AssigneeUserId.Value.ToString(),
                cancellationToken);
        }

        ticket.Assign(assigneeUserId);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ticket.Id;
    }
}