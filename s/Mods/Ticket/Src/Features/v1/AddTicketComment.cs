using FluentValidation;
using FSH.Framework.Core.Context;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Mods.Ticket.Data;
using FSH.Mods.Ticket.Spec;
using FSH.Mods.Ticket.Spec.v1.TicketComment;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace FSH.Mods.Ticket.Features.v1;

public static class AddTicketCommentEndpoint
{
    public sealed record AddTicketCommentRequest(string Body);

    internal static RouteHandlerBuilder MapAddTicketCommentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/tickets/{ticketId:guid}/comments",
                async (Guid ticketId, AddTicketCommentRequest body, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(new AddTicketCommentCommand(ticketId, body.Body), ct)))
            .WithName("AddTicketComment")
            .WithSummary("Add a comment to a ticket")
            .RequirePermission(TicketPermissions.Ticket.Comment)
            .WithIdempotency();
    }
}

public sealed class AddTicketCommentCommandValidator : AbstractValidator<AddTicketCommentCommand>
{
    public AddTicketCommentCommandValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(8192);
    }
}

public sealed class AddTicketCommentCommandHandler(
    TicketDbContext dbContext,
    ICurrentUser currentUser)
    : ICommandHandler<AddTicketCommentCommand, Guid>
{
    public async ValueTask<Guid> Handle(AddTicketCommentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var authorId = currentUser.GetIntUserId();
        if (authorId is null)
        {
            throw new CustomException(
                "Cannot post a comment without an authenticated author.",
                (IEnumerable<string>?)null,
                HttpStatusCode.Unauthorized);
        }

        // Load the Comments collection up front so EF's change tracker detects the new TicketComment
        // (added via the aggregate) as an INSERT rather than missing it during change detection.
        var ticket = await dbContext.Tickets
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == command.TicketId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Ticket {command.TicketId} not found.");

        var commentId = ticket.AddComment(authorId, command.Body);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return commentId;
    }
}