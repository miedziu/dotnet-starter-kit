using System.Text.Json.Serialization;

namespace FSH.Mod.Ticket.Spec.v1;

[JsonConverter(typeof(JsonStringEnumConverter<TicketPriority>))]
public enum TicketPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3,
}