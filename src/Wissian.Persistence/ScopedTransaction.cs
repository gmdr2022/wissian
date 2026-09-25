using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Wissian.Persistence;

// Only the trusted server adapter supplies IDs after authenticating the caller.
// Transaction-local settings prevent authority surviving a pooled connection.
public static class ScopedTransaction
{
    public static Task SetOrganizationAsync(WissianDbContext db, Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty || db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Organização requer transação autenticada.");
        var organization = organizationId.ToString();
        return db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('wissian.organization_id', {organization}, true)", cancellationToken);
    }

    public static async Task<IDbContextTransaction> BeginAsync(WissianDbContext db, Guid actorId,
        Guid? organizationId = null, CancellationToken cancellationToken = default)
    {
        if (actorId == Guid.Empty || organizationId == Guid.Empty)
            throw new ArgumentException("Escopo requer identificadores válidos.");
        var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var actor = actorId.ToString();
            var organization = organizationId?.ToString() ?? "";
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT set_config('wissian.actor_id', {actor}, true), set_config('wissian.organization_id', {organization}, true)", cancellationToken);
            return transaction;
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }
    }
}
