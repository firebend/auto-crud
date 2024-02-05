using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Models;

public class EfCustomFieldsModelTenant<TKey, TEntity, TTenantKey> : EfCustomFieldsModel<TKey, TEntity>, ITenantEntity<TTenantKey>
    where TKey : struct
    where TTenantKey : struct
{
    public TTenantKey TenantId { get; set; }
}
