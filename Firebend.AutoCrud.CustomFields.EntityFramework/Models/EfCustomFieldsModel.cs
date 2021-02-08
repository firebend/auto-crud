using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Models
{
    public class EfCustomFieldsModel<TKey, TEntity> : CustomFieldsEntity<TKey>
            where TKey : struct
        {
            public EfCustomFieldsModel()
            {

            }

            public EfCustomFieldsModel(CustomFieldsEntity<TKey> customFieldsEntity)
            {
                customFieldsEntity.CopyPropertiesTo(this);
            }

            public TEntity Entity { get; set; }

            public CustomFieldsEntity<TKey> ToCustomFields()
            {
                var fields = new CustomFieldsEntity<TKey>();
                this.CopyPropertiesTo(fields);
                return fields;
            }
        }
    }
