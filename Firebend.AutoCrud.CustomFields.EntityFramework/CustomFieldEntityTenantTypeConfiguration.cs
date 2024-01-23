using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Firebend.AutoCrud.CustomFields.EntityFramework;

public class CustomFieldEntityTenantTypeConfiguration<TKey, TEntity, TTenantKey> :
    AbstractCustomFieldEntityTypeConfiguration<TKey, TEntity, EfCustomFieldsModelTenant<TKey, TEntity, TTenantKey>>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
    where TTenantKey : struct
{
    public CustomFieldEntityTenantTypeConfiguration(string tableName, string schema) : base(tableName, schema)
    {
    }

    public override void Configure(EntityTypeBuilder<EfCustomFieldsModelTenant<TKey, TEntity, TTenantKey>> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.TenantId).IsRequired();
    }
}
