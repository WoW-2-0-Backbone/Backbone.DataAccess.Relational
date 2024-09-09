namespace Backbone.DataAccess.Relational.Entities.Models;

/// <summary>
/// Defines an entity that is auditable for deletion tracking.
/// </summary>
public interface IDeletionAuditableEntity
{
    /// <summary>
    /// Gets or sets the ID of the user who deleted the entity.
    /// </summary>
    Guid? DeletedByUserId { get; set; }
}