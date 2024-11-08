using Backbone.Comms.Infra.Abstractions.Commands;
using Backbone.Comms.Infra.Abstractions.Queries;
using Backbone.DataAccess.Relational.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Backbone.DataAccess.Relational.EfCore.Repositories.Repositories;

/// <summary>
/// Represents a base repository for primary entities with common CRUD operations.
/// </summary>
public class PrimaryEntityRepositoryBase<TKey, TEntity, TContext>(TContext dbContext) : EntityRepositoryBase<TEntity, TContext>(dbContext)
    where TEntity : class, IPrimaryEntity<TKey> where TContext : DbContext
{
    /// <summary>
    /// Gets a single entity by ID from a database or local entities' snapshot.
    /// </summary>
    /// <param name="entityId">The ID of entity to query.</param>
    /// <param name="queryOptions">The options to query</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The found entity if exists, otherwise null</returns>
    public virtual async ValueTask<TEntity?> GetByIdAsync(
        TKey entityId,
        QueryOptions queryOptions = default,
        CancellationToken cancellationToken = default
    )
    {
        // Query from local entities snapshot and from database only if not found 
        return Entities.Local.FirstOrDefault(entity => entity.Id!.Equals(entityId))
               ?? await Get(existingEntity => existingEntity.Id!.Equals(entityId), queryOptions).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    /// <param name="entity">Entity to create</param>
    /// <param name="commandOptions">Create command options</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The created entity</returns>
    public override async ValueTask<TEntity> CreateAsync(
        TEntity entity,
        CommandOptions commandOptions = default,
        CancellationToken cancellationToken = default
    )
    {
        await Entities.AddAsync(entity, cancellationToken);

        if (!commandOptions.SkipSavingChanges)
            await DbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    /// <summary>
    /// Deletes an existing entity by ID.
    /// </summary>
    /// <param name="entityId">Id of entity to delete</param>
    /// <param name="commandOptions">Delete command options</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>Deletion result</returns>
    public virtual async ValueTask<TEntity?> DeleteByIdAsync(TKey entityId, CommandOptions commandOptions,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(entityId, cancellationToken: cancellationToken) ??
                     throw new InvalidOperationException();

        DbContext.Remove(entity);

        if (!commandOptions.SkipSavingChanges)
            await DbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }
}