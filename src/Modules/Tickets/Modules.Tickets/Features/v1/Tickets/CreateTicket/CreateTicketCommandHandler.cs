using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using FSH.Modules.Identity.Contracts.Services;
using FSH.Modules.Tickets.Contracts.v1.Tickets;
using FSH.Modules.Tickets.Data;
using FSH.Modules.Tickets.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;

namespace FSH.Modules.Tickets.Features.v1.Tickets.CreateTicket;

public sealed class CreateTicketCommandHandler(
    TicketsDbContext dbContext,
    IUserProfileService userProfileService,
    ICurrentUser currentUser)
    : ICommandHandler<CreateTicketCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateTicketCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var reporterId = currentUser.GetIntUserId();
        if (reporterId is null)
        {
            throw new CustomException(
                "Cannot create a ticket without an authenticated reporter.",
                (IEnumerable<string>?)null,
                HttpStatusCode.Unauthorized);
        }

        // Sequential, tenant-scoped ticket numbers (TK-1, …). Count ALL rows incl. soft-deleted so a
        // deleted number isn't reused; racing writers collide on the unique index (→ 409, retryable).
        long count = await dbContext.Tickets
            .IgnoreQueryFilters([QueryFilters.SoftDelete])
            .LongCountAsync(cancellationToken)
            .ConfigureAwait(false);
        string number = $"TK-{(count + 1).ToString(CultureInfo.InvariantCulture)}";

        int? assignedToUserId = null;
        if (command.AssignedToUserId.HasValue)
        {
            assignedToUserId = await userProfileService.GetIntIdAsync(
                command.AssignedToUserId.Value.ToString(),
                cancellationToken);
        }

        var ticket = Ticket.Create(
            number: number,
            title: command.Title,
            description: command.Description,
            priority: command.Priority,
            reporterUserId: reporterId,
            assignedToUserId: assignedToUserId);

        dbContext.Tickets.Add(ticket);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ticket.Id;
    }
}