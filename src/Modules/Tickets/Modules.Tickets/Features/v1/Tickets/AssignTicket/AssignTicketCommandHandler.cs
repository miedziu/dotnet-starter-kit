using FSH.Framework.Core.Exceptions;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Tickets.Contracts.v1.Tickets;
using FSH.Modules.Tickets.Data;
using FSH.Modules.Tickets.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Tickets.Features.v1.Tickets.AssignTicket;

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