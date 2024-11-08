using System.Linq.Expressions;
using Backbone.Comms.Infra.Abstractions.Commands;
using Backbone.Comms.Infra.Abstractions.Queries;
using Backbone.DataAccess.Relational.EfCore.Abstractions.Extensions;
using Backbone.DataAccess.Relational.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Backbone.DataAccess.Relational.EfCore.Repositories.Repositories;

/// <summary>
/// Represents a base repository for entities with common CRUD operations.
/// </summary>
public class EntityRepositoryBase<TEntity, TContext>(TContext dbContext) where TEntity : class, IEntity where TContext : DbContext
{
    /// <summary>
    /// Gets the database context instance.
    /// </summary>
    protected TContext DbContext => dbContext;

    /// <summary>
    /// Gets entities set.
    /// </summary>
    protected DbSet<TEntity> Entities => dbContext.Set<TEntity>();

    /// <summary>
    /// Gets entities queryable source based on optional filtering conditions
    /// </summary>
    /// <param name="predicate">Entity filter predicate</param>
    /// <param name="queryOptions">Query options</param>
    /// <returns>Queryable source of entities</returns>
    public virtual IQueryable<TEntity> Get(Expression<Func<TEntity, bool>>? predicate = default, QueryOptions queryOptions = default)
    {
        var initialQuery = Entities.Where(entity => true);

        if (predicate is not null)
            initialQuery = initialQuery.Where(predicate);

        return initialQuery.ApplyTrackingMode(queryOptions.TrackingMode);
    }

    /// <summary>
    /// Checks if an entity exists.
    /// </summary>
    /// <param name="queryableSource">Queryable source of the entities.</param>
    /// <param name="expectedValue">Expected value to check against the result</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>True if entity exists, otherwise false</returns>
    public virtual async ValueTask<bool> CheckAsync<TValue>(
        IQueryable<TValue> queryableSource, 
        TValue? expectedValue = default,
        CancellationToken cancellationToken = default)
    {
        var result = await queryableSource.FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return result is not null && (expectedValue is not null
            ? result.Equals(expectedValue)
            : !result.Equals(default(TValue)));
    }

    /// <summary>
    /// Checks if entity exists and returns a specified property value.
    /// </summary>
    /// <param name="predicate">Predicate to check entity existence</param>
    /// <param name="memberSelector">Expression that selects the member of entity as a result</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>True if entity exists, otherwise false</returns>
    public virtual async ValueTask<(bool Result, TProperty Property)> CheckAsync<TProperty>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TProperty>> memberSelector,
        CancellationToken cancellationToken = default
    )
    {
        var foundResult = await Entities
            .Where(predicate)
            .Select(memberSelector)
            .FirstOrDefaultAsync(cancellationToken);

        var isDefault = EqualityComparer<TProperty>.Default.Equals(foundResult, default);

        return (!isDefault, foundResult!);
    }

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    /// <param name="entity">Entity to create</param>
    /// <param name="commandOptions">Create command options</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The created entity</returns>
    public virtual async ValueTask<TEntity> CreateAsync(
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
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <param name="commandOptions">Update command options</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The Updated entity</returns>
    public virtual async ValueTask<TEntity> UpdateAsync(TEntity entity, CommandOptions commandOptions,
        CancellationToken cancellationToken = default)
    {
        Entities.Update(entity);

        if (!commandOptions.SkipSavingChanges)
            await DbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    /// <summary>
    /// Updates entities in batch.
    /// </summary>
    /// <param name="batchUpdatePredicate">Predicate to select entities for batch update</param>
    /// <param name="setPropertyCalls">Batch update value selectors</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>Number of updated rows.</returns>
    public virtual async ValueTask<int> UpdateBatchAsync(
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls,
        Expression<Func<TEntity, bool>>? batchUpdatePredicate = default,
        CancellationToken cancellationToken = default
    )
    {
        var entities = Entities.AsQueryable();

        if (batchUpdatePredicate is not null)
            entities = entities.Where(batchUpdatePredicate);

        return await entities.ExecuteUpdateAsync(setPropertyCalls, cancellationToken);
    }

    /// <summary>
    /// Deletes an existing entity by ID.
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <param name="commandOptions">Delete command options</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>Updated entity if soft deleted, otherwise null</returns>
    public virtual async ValueTask<TEntity?> DeleteAsync(
        TEntity entity,
        CommandOptions commandOptions = default,
        CancellationToken cancellationToken = default
    )
    {
        Entities.Remove(entity);

        if (!commandOptions.SkipSavingChanges)
            await DbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    /// <summary>
    /// Deletes entities in batch.
    /// </summary>
    /// <param name="source">A function that selects the entities to be deleted.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    public virtual async ValueTask DeleteBatchAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> source,
        CancellationToken cancellationToken = default)
    {
        await source(Entities).ExecuteDeleteAsync(cancellationToken: cancellationToken);
    }
}