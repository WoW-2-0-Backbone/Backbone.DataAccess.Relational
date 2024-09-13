using Backbone.Comms.Infra.Abstractions.Commands;
using Backbone.Comms.Infra.Abstractions.Queries;
using Backbone.DataAccess.Relational.EfCore.Abstractions.Extensions;
using Backbone.DataAccess.Relational.EfCore.Repositories.Repositories;
using Backbone.DataAccess.Relational.Entities.Models;
using Backbone.Storage.Cache.Abstractions.Brokers;
using Backbone.Storage.Cache.Abstractions.Models;
using Microsoft.EntityFrameworkCore;

namespace Backbone.DataAccess.Relational.EfCore.Repositories.Cached.Repositories;

/// <summary>
/// Represents a base repository with caching for entities with common CRUD operations.
/// </summary>
public class CachedPrimaryEntityRepositoryBase<TEntity, TContext>(
    TContext dbContext,
    ICacheStorageBroker cacheStorageBroker,
    CacheEntryOptions? cacheEntryOptions = default
) : PrimaryEntityRepositoryBase<TEntity, TContext>(dbContext)
    where TEntity : class, IPrimaryEntity, ICacheEntry where TContext : DbContext
{
    /// <summary>
    /// Gets cache storage broker instance.
    /// </summary>
    protected readonly ICacheStorageBroker CacheStorageBroker = cacheStorageBroker;
    
    /// <summary>
    /// Gets an entity by ID from database or cache storage.
    /// </summary>
    /// <param name="entityId">Entity ID</param>
    /// <param name="queryOptions">Query options</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>Entity if found, otherwise null</returns>
    protected override async ValueTask<TEntity?> GetByIdAsync(
        Guid entityId,
        QueryOptions queryOptions = default,
        CancellationToken cancellationToken = default
    )
    {
        // Query from local entities snapshot storage.
        var entity = Entities.Local.FirstOrDefault(entity => entity.Id == entityId);
        if (entity is not null)
            return entity;

        // Query from cache storage.
        entity = await CacheStorageBroker.GetOrSetAsync(
            entityId.ToString(),
            async ct => await Get(existingEntity => existingEntity.Id == entityId, queryOptions)
                .FirstOrDefaultAsync(ct),
            cacheEntryOptions,
            entry =>
            {
                if (entry is not null)
                    DbContext.Entry(entry).ApplyTrackingMode(queryOptions.TrackingMode);
            },
            cancellationToken
        );

        return entity;
    }

    /// <summary>
    /// Creates a new entity
    /// </summary>
    /// <param name="entity">Entity to create</param>
    /// <param name="commandOptions">Create command options</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>Created entity</returns>
    protected override async ValueTask<TEntity> CreateAsync(
        TEntity entity,
        CommandOptions commandOptions = default,
        CancellationToken cancellationToken = default
    )
    {
        // Save to a database storage.
        await base.CreateAsync(entity, commandOptions, cancellationToken);

        // Save to cache storage.
        await CacheStorageBroker.SetAsync(entity.Id.ToString(), entity, cacheEntryOptions, cancellationToken);

        return entity;
    }

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <param name="commandOptions">Update command options</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>Updated entity</returns>
    protected override async ValueTask<TEntity> UpdateAsync(TEntity entity, CommandOptions commandOptions,
        CancellationToken cancellationToken = default)
    {
        // Save to a database storage.
        await base.UpdateAsync(entity, commandOptions, cancellationToken);

        // Save to cache storage.
        await CacheStorageBroker.SetAsync(entity.Id.ToString(), entity, cacheEntryOptions, cancellationToken);

        return entity;
    }

    /// <summary>
    /// Deletes an existing entity.
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <param name="commandOptions">Delete command options</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>Updated entity if soft deleted, otherwise null</returns>
    protected override async ValueTask<TEntity?> DeleteAsync(
        TEntity entity,
        CommandOptions commandOptions = default,
        CancellationToken cancellationToken = default
    )
    {
        // Save to a database storage.
        await base.DeleteAsync(entity, commandOptions, cancellationToken);

        // Save to cache storage.
        await CacheStorageBroker.DeleteAsync(entity.Id.ToString(), cancellationToken);

        return entity;
    }

    /// <summary>
    /// Deletes an existing entity by ID.
    /// </summary>
    /// <param name="entityId">ID of entity to delete</param>
    /// <param name="commandOptions">Delete command options</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>Deletion result</returns>
    protected override async ValueTask<TEntity?> DeleteByIdAsync(Guid entityId, CommandOptions commandOptions,
        CancellationToken cancellationToken = default)
    {
        // Save to a database storage.
        var entity = await DeleteAsync(await GetByIdAsync(entityId, cancellationToken: cancellationToken) ??
                                       throw new InvalidOperationException(), commandOptions, cancellationToken);

        // Save to cache storage.
        await CacheStorageBroker.DeleteAsync(entity!.Id.ToString(), cancellationToken);

        return entity;
    }

    /// <summary>
    /// Deletes entities in batch.
    /// </summary>
    /// <param name="source">A function that selects the entities to be deleted.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    protected override async ValueTask DeleteBatchAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> source,
        CancellationToken cancellationToken = default)
    {
        // Query matching entities ID
        var entitiesId = await source(Entities).Select(entity => entity.Id).ToListAsync(cancellationToken);
        if (!entitiesId.Any()) return;

        // Remove from cache
        await Task.WhenAll(entitiesId.Select(entityId => CacheStorageBroker.DeleteAsync(entityId.ToString(), cancellationToken).AsTask()));

        // Remove from database
        await Entities.Where(entity => entitiesId.Contains(entity.Id)).ExecuteDeleteAsync(cancellationToken);
    }
}