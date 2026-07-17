using FluentValidation;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Tickets.Contracts.Authorization;
using FSH.Modules.Tickets.Contracts.v1.Dtos;
using FSH.Modules.Tickets.Contracts.v1.Tickets;
using FSH.Modules.Tickets.Data;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Tickets.Features.v1;

public static class UpdateTicketEndpoint
{
    public sealed record UpdateTicketRequest(string Title, string? Description, TicketPriority Priority);

    internal static RouteHandlerBuilder MapUpdateTicketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/tickets/{ticketId:guid}",
                async (Guid ticketId, UpdateTicketRequest body, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(
                        new UpdateTicketCommand(ticketId, body.Title, body.Description, body.Priority), ct)))
            .WithName("UpdateTicket")
            .WithSummary("Edit a ticket's title, description, and priority")
            .RequirePermission(TicketsPermissions.Tickets.Update)
            .WithIdempotency();
    }
}

public sealed class UpdateTicketCommandValidator : AbstractValidator<UpdateTicketCommand>
{
    public UpdateTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Description).MaximumLength(4096);
        RuleFor(x => x.Priority).IsInEnum();
    }
}

public sealed class UpdateTicketCommandHandler(TicketsDbContext dbContext)
    : ICommandHandler<UpdateTicketCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateTicketCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var ticket = await dbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Ticket {command.TicketId} not found.");

        ticket.UpdateDetails(command.Title, command.Description, command.Priority);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return ticket.Id;
    }
}