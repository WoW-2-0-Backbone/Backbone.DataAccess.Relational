namespace Backbone.DataAccess.Relational.Entities.Events;

/// <summary>
/// Defines entity event types.
/// </summary>
public enum EntityEventType
{
    /// <summary>
    /// Refers to querying events.
    /// </summary>
    OnGet,

    /// <summary>
    /// Refers to creating a new entity.
    /// </summary>
    OnCreate,

    /// <summary>
    /// Refers to updating an entity.
    /// </summary>
    OnUpdate,

    /// <summary>
    /// Refers to cloning an entity.
    /// </summary>
    OnClone,

    /// <summary>
    /// Refers to deleting an entity.
    /// </summary>
    OnDelete,
}