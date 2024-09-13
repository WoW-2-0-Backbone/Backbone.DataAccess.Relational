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
public class CachedEntityRepositoryBase<TEntity, TContext>(
    TContext dbContext,
    ICacheStorageBroker cacheStorageBroker,
    CacheEntryOptions? cacheEntryOptions = default
) : EntityRepositoryBase<TEntity, TContext>(dbContext) where TEntity : class, IEntity, ICacheEntry where TContext : DbContext
{
    /// <summary>
    /// Gets cache storage broker instance.
    /// </summary>
    protected readonly ICacheStorageBroker CacheStorageBroker = cacheStorageBroker;

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
        await CacheStorageBroker.SetAsync(entity.CacheKey, entity, cacheEntryOptions, cancellationToken);

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
        await CacheStorageBroker.SetAsync(entity.CacheKey, entity, cacheEntryOptions, cancellationToken);

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
        await CacheStorageBroker.DeleteAsync(entity.CacheKey, cancellationToken);

        return entity;
    }
}