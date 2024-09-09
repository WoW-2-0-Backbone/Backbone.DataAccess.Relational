namespace Backbone.DataAccess.Relational.Entities.Models;

/// <summary>
/// Defines an entity that is auditable for creation tracking.
/// </summary>
public interface ICreationAuditableEntity
{
    /// <summary>
    /// Gets or sets the ID of the user who created the entity.
    /// </summary>
    Guid CreatedByUserId { get; set; }
}