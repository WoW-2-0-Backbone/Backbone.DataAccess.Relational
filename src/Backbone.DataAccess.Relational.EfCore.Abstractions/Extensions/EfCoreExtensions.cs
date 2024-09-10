using Backbone.Comms.Infra.Abstractions.Queries;
using Backbone.DataAccess.Relational.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Backbone.DataAccess.Relational.EfCore.Abstractions.Extensions;

/// <summary>
/// Contains EF core internal logic extensions.
/// </summary>
public static class EfCoreExtensions
{
    /// <summary>
    /// Applies tracking mode for queryable source of entities.
    /// </summary>
    /// <param name="source">Queryable source</param>
    /// <param name="trackingMode">Tracking mode to apply</param>
    /// <typeparam name="TSource">Query source type</typeparam>
    /// <returns>Query source with tracking mode applied</returns>
    public static IQueryable<TSource> ApplyTrackingMode<TSource>(this IQueryable<TSource> source, QueryTrackingMode trackingMode)
        where TSource : class
    {
        return trackingMode switch
        {
            QueryTrackingMode.AsTracking => source.AsTracking(),
            QueryTrackingMode.AsNoTracking => source.AsNoTracking(),
            QueryTrackingMode.AsNoTrackingWithIdentityResolution => source.AsNoTrackingWithIdentityResolution(),
            _ => source
        };
    }

    /// <summary>
    /// Applies tracking mode to the entity entry.
    /// </summary>
    /// <param name="entityEntry">Entity entry in db context to track</param>
    /// <param name="trackingMode">The tracking mode to apply</param>
    /// <typeparam name="TEntity">The type of entity entry</typeparam>
    /// <returns>Entity entry with tracking mode applied</returns>
    public static EntityEntry<TEntity> ApplyTrackingMode<TEntity>(this EntityEntry<TEntity> entityEntry, QueryTrackingMode trackingMode)
        where TEntity : class
    {
        entityEntry.State = trackingMode switch
        {
            QueryTrackingMode.AsNoTracking or QueryTrackingMode.AsNoTrackingWithIdentityResolution => EntityState.Detached,
            QueryTrackingMode.AsTracking => EntityState.Unchanged,
            _ => entityEntry.State
        };

        return entityEntry;
    }

    /// <summary>
    /// Queries the source and sets a filter of matched entities IDs to given different queryable source.
    /// </summary>
    /// <param name="source">Original query source</param>
    /// <param name="filteredSource">Query source to set filter of matched entities IDs.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <typeparam name="TSource">Query source type</typeparam>
    /// <returns>Query source with filter of matched entities IDs set.</returns>
    public static async ValueTask<IQueryable<TSource>> GetFilteredEntitiesQueryAsync<TSource>(
        this IQueryable<TSource> filteredSource,
        IQueryable<TSource> source,
        CancellationToken cancellationToken = default
    ) where TSource : class, IEntity
    {
        var entitiesId = await filteredSource.Select(entity => entity.Id).ToListAsync(cancellationToken: cancellationToken);
        return source.Where(entity => entitiesId.Contains(entity.Id));
    }
}