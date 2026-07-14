# Module: Tickets

Support ticket lifecycle with comments. Module `Order = 700`.

**Entities:** `Ticket` (soft-deletable, state machine) + `TicketComment`. `TicketsDbContext`. `TicketStatus`/`TicketPriority` enums in Contracts; domain events internal.

**Areas:** Create, Assign, Resolve, Reopen, Restore, AddComment, ListComments, GetById, Search, ListTrashed.

## Gotchas

- **State machine** (`Domain/Ticket.cs`): `Open → InProgress → Resolved → Closed`. Illegal transitions throw **`CustomException` with `HttpStatusCode.Conflict` (409)** — not generic 400. Assigning auto-starts (Open→InProgress); unassigning InProgress reverts to Open; creating with assignee starts at InProgress. Closed tickets reject comments/resolve until reopened.
- Soft-delete/restore/trash identical to Catalog (filtered unique indexes).
- Endpoints mapped on bare `api/v{version}` group (no `/tickets` sub-path); literal routes precede `{ticketId:guid}`.