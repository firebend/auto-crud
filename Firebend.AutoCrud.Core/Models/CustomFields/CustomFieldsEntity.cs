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

        public TKey EntityId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        [Key]
        public Guid Id { get; set; }

        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
    }
}
