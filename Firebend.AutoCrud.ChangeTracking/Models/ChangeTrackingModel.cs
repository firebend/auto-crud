using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Firebend.AutoCrud.ChangeTracking.Models;

public class ChangeTrackingModel<TKey, TEntity> : IEntity<Guid>, IModifiedEntity
    where TEntity : class
{
    /// <summary>
    /// Gets or sets a value indicating the user who made the change.
    /// </summary>
    public string UserEmail { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the set of changes made to the entity.
    /// </summary>
    public List<Operation> Changes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating descriptions of where the change was made at.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets a value indicating what action was taken on the entity. Add, Update, Delete.
    /// </summary>
    public string Action { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the entity's unique identifier.
    /// </summary>
    public TKey EntityId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the entity's original form before the changes were made.
    /// </summary>
    public TEntity Entity { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the id corresponding to the change tracking record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the time the change tracking record was modified.
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the time the change tracking record was created.
    /// </summary>
    public DateTimeOffset ModifiedDate { get; set; }
}
