using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Models
{
    public interface IEfCustomFieldsModel<TKey>
        where TKey : struct
    {
        CustomFieldsEntity<TKey> ToCustomFields();
    }

    public class EfCustomFieldsModel<TKey, TEntity> : CustomFieldsEntity<TKey>, IEfCustomFieldsModel<TKey>, IEntityName
        where TKey : struct
    {
        public TEntity Entity { get; set; }

        public CustomFieldsEntity<TKey> ToCustomFields()
        {
            var fields = new CustomFieldsEntity<TKey>();
            this.CopyPropertiesTo(fields);
            return fields;
        }

        public string GetEntityName()
        {
            var entityType = typeof(TEntity);

            var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();
            var tableName = tableAttribute?.Name;

            if (string.IsNullOrWhiteSpace(tableName))
            {
                tableName = entityType.Name;
            }

            return $"{tableName}_CustomFields";
        }
    }

    public class EfCustomFieldsModelTenant<TKey, TEntity, TTenantKey> : EfCustomFieldsModel<TKey, TEntity>, ITenantEntity<TTenantKey>
        where TKey : struct
        where TTenantKey : struct
    {
        public TTenantKey TenantId { get; set; }
    }
}
