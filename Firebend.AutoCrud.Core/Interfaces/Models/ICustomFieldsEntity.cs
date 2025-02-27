using System.Collections.Generic;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface ICustomFieldsEntity<TKey>
    where TKey : struct
{
    public List<CustomFieldsEntity<TKey>> CustomFields { get; set; }
}
