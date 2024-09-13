using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backbone.DataAccess.Relational.EfCore.Abstractions.Extensions;

/// <summary>
/// Contains extensions for database migration.
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Migrates the database associated with the specified context.
    /// </summary>
    /// <param name="scopeFactory">Service scope factory</param>
    /// <typeparam name="TContext">Data access context</typeparam>
    public static async ValueTask MigrateAsync<TContext>(this IServiceScopeFactory scopeFactory) where TContext : DbContext
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        if ((await context.Database.GetPendingMigrationsAsync()).Any())
            await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Migrates the database associated with the specified context.
    /// </summary>
    /// <param name="scopeFactory">Service scope factory</param>
    /// <param name="dbContextTypes">Collection of DbContext types to migrate</param>
    public static async ValueTask MigrateAsync(this IServiceScopeFactory scopeFactory, ICollection<Type> dbContextTypes)
    {
        if (dbContextTypes.Any(type => !type.IsAssignableTo(typeof(DbContext))))
            throw new ArgumentException("All types must inherit from DbContext to do database migration", nameof(dbContextTypes));

        await Task.WhenAll(dbContextTypes
            .Select(async type =>
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var context = (scope.ServiceProvider.GetRequiredService(type) as DbContext)!;
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                    await context.Database.MigrateAsync();
            }));
    }
}