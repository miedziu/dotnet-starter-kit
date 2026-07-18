using FluentValidation;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Mod.Ticket.Data;
using FSH.Mod.Ticket.Spec.v1.Ticket;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Mod.Ticket.Features.v1;

public static class DeleteTicketEndpoint
{
    internal static RouteHandlerBuilder MapDeleteTicketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/tickets/{ticketId:guid}",
                async (Guid ticketId, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteTicketCommand(ticketId), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteTicket")
            .WithSummary("Soft-delete a ticket (restorable from trash)")
            .RequirePermission(TicketPermissions.Ticket.Delete);
    }
}

public sealed class DeleteTicketCommandValidator : AbstractValidator<DeleteTicketCommand>
{
    public DeleteTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
    }
}

public sealed class DeleteTicketCommandHandler(TicketDbContext dbContext)
    : ICommandHandler<DeleteTicketCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteTicketCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ticket = await dbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Ticket {command.TicketId} not found.");

        // Soft delete: the audit interceptor converts the EF Delete into an IsDeleted flip.
        // Comments are not auto-included, so they are left untouched and survive a Restore.
        dbContext.Tickets.Remove(ticket);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}