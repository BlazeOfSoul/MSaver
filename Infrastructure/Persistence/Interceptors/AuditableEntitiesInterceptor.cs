using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MSaver.Infrastructure.Persistence.Interceptors;

public sealed class AuditableEntitiesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            UpdateAuditFields(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            UpdateAuditFields(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditFields(DbContext context)
    {
        var utcNow = DateTime.UtcNow;

        List<EntityEntry<AuditableEntity>> entries = context.ChangeTracker
            .Entries<AuditableEntity>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(x => x.CreatedAt).CurrentValue = utcNow;
                entry.Property(x => x.UpdatedAt).CurrentValue = utcNow;
                continue;
            }

            entry.Property(x => x.CreatedAt).IsModified = false;
            entry.Property(x => x.UpdatedAt).CurrentValue = utcNow;
        }
    }
}