using Mediator;

namespace FSH.Modules.Notifications.Contracts.v1;

/// <summary>Bell badge count — number of caller's unread notifications.</summary>
public sealed record GetUnreadCountQuery : IQuery<int>;