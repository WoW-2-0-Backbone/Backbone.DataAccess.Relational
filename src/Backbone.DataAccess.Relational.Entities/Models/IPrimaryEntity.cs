namespace Backbone.DataAccess.Relational.Entities.Models;

/// <summary>
/// Defines an entity abstraction with entity ID.
/// </summary>
public interface IPrimaryEntity<TKey> : IEntity
{
    /// <summary>
    /// Gets or sets the entity ID.
    /// </summary>
    public TKey Id { get; set; }
}