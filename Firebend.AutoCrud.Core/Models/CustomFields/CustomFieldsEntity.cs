using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.CustomFields
{
    public class CustomFieldsEntity<TKey> : IEntity<Guid>
        where TKey : struct
    {
        public TKey EntityId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public Guid Id { get; set; }
    }
}
