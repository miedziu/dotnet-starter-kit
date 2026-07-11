using FSH.Framework.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace FSH.Framework.Persistence.Context;

/// <summary>
/// Base database context with soft delete support.
/// </summary>
/// <param name="options">Database context options.</param>
public class BaseDbContext(
    DbContextOptions options)
    : DbContext(options)
{

    /// <summary>
    /// Configures the model by applying global query filters for soft delete functionality.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure the database schema.</param>
    /// <exception cref="ArgumentNullException">Thrown when modelBuilder is null.</exception>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AppendGlobalQueryFilter<ISoftDeletable>(QueryFilters.SoftDelete, s => !s.IsDeleted);
    }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the save operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }
}