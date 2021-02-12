using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public abstract class AbstractTenantSqlServerCustomFieldsStorageCreator<TKey, TEntity, TTenantKey, TEfModelType> :
        AbstractSqlServerCustomFieldsStorageCreator<TKey, TEntity, TEfModelType>
        where TKey : struct
        where TTenantKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>, ITenantEntity<TTenantKey>
        where TEfModelType : EfCustomFieldsModel<TKey, TEntity>
    {
        private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

        protected AbstractTenantSqlServerCustomFieldsStorageCreator(IDbContextProvider<TKey, TEntity> contextProvider,
            IEntityTableCreator tableCreator,
            ITenantEntityProvider<TTenantKey> tenantEntityProvider)
            : base(contextProvider, tableCreator)
        {
            _tenantEntityProvider = tenantEntityProvider;
        }

        protected override async Task<string> GetCacheKey(CancellationToken cancellationToken)
        {
            var tenant = await _tenantEntityProvider.GetTenantAsync(cancellationToken).ConfigureAwait(false);

            return $"{tenant.TenantId}_{typeof(TEntity).Name}";
        }
    }
}
