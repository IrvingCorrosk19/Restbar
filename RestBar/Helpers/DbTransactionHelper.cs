using Microsoft.EntityFrameworkCore;

namespace RestBar.Helpers;

/// <summary>
/// Ejecuta trabajo transaccional compatible con NpgsqlRetryingExecutionStrategy
/// (EnableRetryOnFailure). Las transacciones manuales deben ir dentro de
/// Database.CreateExecutionStrategy().
/// </summary>
public static class DbTransactionHelper
{
    public static Task ExecuteInTransactionAsync(DbContext context, Func<Task> operation)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await operation();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public static Task<T> ExecuteInTransactionAsync<T>(DbContext context, Func<Task<T>> operation)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var result = await operation();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    /// <summary>
    /// Para flujos que hacen commit/rollback manual (p. ej. early-return con BadRequest).
    /// El caller controla Commit/Rollback; este método solo envuelve con execution strategy.
    /// </summary>
    public static Task ExecuteWithStrategyAsync(DbContext context, Func<Task> operation)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(operation);
    }

    public static Task<T> ExecuteWithStrategyAsync<T>(DbContext context, Func<Task<T>> operation)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(operation);
    }
}
