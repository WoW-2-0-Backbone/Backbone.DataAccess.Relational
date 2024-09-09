namespace Backbone.DataAccess.Relational.Entities.Models;

/// <summary>
/// Defines an entity that supports soft deletion.
/// </summary>
public interface ISoftDeletedEntity
{
    /// <summary>
    /// Gets or sets entity deleted indicator.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets entity deleted time.
    /// </summary>
    public DateTimeOffset? DeletedTime { get; set; }
}