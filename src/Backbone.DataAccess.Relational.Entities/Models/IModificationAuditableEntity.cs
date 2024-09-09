namespace Backbone.DataAccess.Relational.Entities.Models;

/// <summary>
/// Defines an entity that is auditable for modification owner tracking.
/// </summary>
public interface IModificationAuditableEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// </summary>
    public Guid ModifiedByUserId { get; set; }
}