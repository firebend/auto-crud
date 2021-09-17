using System;
using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.CustomFields
{
    public class CustomFieldsEntity<TKey> : IEntity<Guid>, IModifiedEntity
        where TKey : struct
    {
        public CustomFieldsEntity()
        {

        }

        public CustomFieldsEntity(CustomFieldsEntity<TKey> customFieldsEntity)
        {
            customFieldsEntity?.CopyPropertiesTo(this);
        }

        /// <summary>
        /// The id of the entity the custom field is for.
        /// </summary>
        public TKey EntityId { get; set; }

        /// <summary>
        /// The key of the custom field.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The value of the custom field.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The custom field's Id
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// The date the custom field was created.
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date the custom field was modified.
        /// </summary>
        public DateTimeOffset ModifiedDate { get; set; }
    }
}
