using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;

namespace Firebend.AutoCrud.CustomFields.EntityFramework
{
    public class CustomFieldEntityTypeConfiguration<TKey, TEntity> :
        AbstractCustomFieldEntityTypeConfiguration<TKey, TEntity, EfCustomFieldsModel<TKey, TEntity>>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        public CustomFieldEntityTypeConfiguration(string tableName, string schema) : base(tableName, schema)
        {
        }
    }
}
