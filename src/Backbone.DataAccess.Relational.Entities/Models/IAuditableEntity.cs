namespace Backbone.DataAccess.Relational.Entities.Models;

/// <summary>
/// Defines an auditable entity with creation and modification tracks.
/// </summary>
public interface IAuditableEntity : IEntity
{
    /// <summary>
    /// Gets or sets entity creation time.
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; }

    /// <summary>
    /// Gets or sets entity modification time.
    /// </summary>
    public DateTimeOffset? ModifiedTime { get; set; }
}