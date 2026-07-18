using Mediator;
using System.Collections.ObjectModel;

namespace FSH.Mod.File.Spec.v1.File;

/// <summary>
/// List files marked Public and tagged with the built-in owner types
/// (<c>MyFiles</c>, <c>User</c>) — the inverse of <c>ListMyFilesQuery</c>. Domain-bound
/// attachments (Product images, Ticket files, Chat messages) deliberately don't show up
/// here: their visibility is a function of their owning entity's access rules, not a
/// free-standing share decision.
/// </summary>
public sealed record ListSharedFilesQuery(int Page = 1, int PageSize = 20)
    : IQuery<ReadOnlyCollection<FileAssetDto>>;