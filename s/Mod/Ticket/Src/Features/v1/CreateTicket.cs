using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Persistence;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Mod.Identity.Spec.Services;
using FSH.Mod.Ticket.Data;
using FSH.Mod.Ticket.Spec.v1.Ticket;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;

namespace FSH.Mod.Ticket.Features.v1;

public static class CreateTicketEndpoint
{
    internal static RouteHandlerBuilder MapCreateTicketEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/tickets",
                async (CreateTicketCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateTicket")
            .WithSummary("Create a ticket")
            .RequirePermission(TicketPermissions.Ticket.Create)
            .WithIdempotency();
    }
}

public sealed class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Description).MaximumLength(4096);
    }
}

public sealed class CreateTicketCommandHandler(
    TicketDbContext dbContext,
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