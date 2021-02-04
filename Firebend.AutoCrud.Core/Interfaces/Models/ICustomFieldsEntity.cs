using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Core.Interfaces.Models
{
    public interface ICustomFieldsEntity<TKey>
        where TKey : struct
    {
        List<CustomFieldsEntity<TKey>> CustomFields { get; set; }
    }

    public interface ICustomFieldsEntity<TKey, TEntity> : ICustomFieldsEntity<TKey>
        where TKey : struct
    {
        List<CustomFieldsEntity<TKey, TEntity>> CustomFields { get; set; }

        List<CustomFieldsEntity<TKey>> ICustomFieldsEntity<TKey>.CustomFields
        {
            get => CustomFields?.Cast<CustomFieldsEntity<TKey>>().ToList();
            set => CustomFields = value?.Select(x =>
                {
                    var e = new CustomFieldsEntity<TKey, TEntity>();
                    x.CopyPropertiesTo(e);
                    return e;
                })
                .ToList();
        }
    }
}
