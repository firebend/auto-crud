using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Firebend.AutoCrud.IntegrationTests.Models
{
    public class ChangeTrackingResponseModel<TKey, TEntity>
        where TEntity : class
    {
        /// <summary>
        ///     The user who made the change.
        /// </summary>
        public string UserEmail { get; set; }

        /// <summary>
        ///     The set of changes made to the entity.
        /// </summary>
        public List<Operation<TEntity>> Changes { get; set; }

        /// <summary>
        ///     A descriptions of where the change was made at.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     What action was taken on the entity. Add, Update, Delete.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        ///     The time the change tracking record was modified.
        /// </summary>
        public DateTimeOffset Modified { get; set; }

        /// <summary>
        ///     An indexable entity id.
        /// </summary>
        public TKey EntityId { get; set; }

        /// <summary>
        ///     The entity's original form before the changes were made.
        /// </summary>
        public TEntity Entity { get; set; }

        /// <summary>
        ///     The id corresponding to the change tracking record.
        /// </summary>
        public Guid Id { get; set; }
    }
}
