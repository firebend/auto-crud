using System;
using System.ComponentModel.DataAnnotations;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.CustomFields
{
    public class CustomFieldsEntity<TKey> : IEntity<Guid>
        where TKey : struct
    {
        public TKey EntityId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        [Key]
        public Guid Id { get; set; }
    }

    public class CustomFieldsEntity<TKey, TEntity> : CustomFieldsEntity<TKey>
        where TKey : struct
    {
        public TEntity Entity { get; set; }
    }
}
