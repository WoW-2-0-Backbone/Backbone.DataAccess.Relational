namespace Backbone.DataAccess.Relational.Entities.Models;

/// <summary>
/// Defines an entity abstraction with entity ID.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets or sets the entity ID.
    /// </summary>
    public Guid Id { get; set; }
}