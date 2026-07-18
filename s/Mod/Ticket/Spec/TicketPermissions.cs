using FSH.Framework.Shared.Constants;

namespace FSH.Mod.Ticket.Spec;

public static class TicketPermissions
{
    public static class Ticket
    {
        public const string Resource = "Ticket";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
        public const string Restore = $"Permissions.{Resource}.Restore";
        public const string Assign = $"Permissions.{Resource}.Assign";
        public const string Resolve = $"Permissions.{Resource}.Resolve";
        public const string Reopen = $"Permissions.{Resource}.Reopen";
        public const string Close = $"Permissions.{Resource}.Close";
        public const string Comment = $"Permissions.{Resource}.Comment";
    }

    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("View Tickets",    ActionConstants.View,    Ticket.Resource, IsBasic: true),
        new("Create Tickets",  ActionConstants.Create,  Ticket.Resource),
        new("Update Tickets",  ActionConstants.Update,  Ticket.Resource),
        new("Delete Tickets",  ActionConstants.Delete,  Ticket.Resource),
        new("Restore Tickets", ActionConstants.Restore, Ticket.Resource),
        new("Assign Tickets",  ActionConstants.Assign,  Ticket.Resource),
        new("Resolve Tickets", ActionConstants.Resolve, Ticket.Resource),
        new("Reopen Tickets",  ActionConstants.Reopen,  Ticket.Resource),
        new("Close Tickets",   ActionConstants.Close,   Ticket.Resource),
        new("Comment on Tickets", ActionConstants.Comment, Ticket.Resource),
    ];
}