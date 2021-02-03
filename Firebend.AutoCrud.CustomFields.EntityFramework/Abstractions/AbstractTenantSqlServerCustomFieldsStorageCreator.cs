using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public class AbstractTenantSqlServerCustomFieldsStorageCreator<TKey, TEntity, TTenantKey> : AbstractSqlServerCustomFieldsStorageCreator<TKey, TEntity>
        where TKey : struct
        where TTenantKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>, ITenantEntity<TTenantKey>
    {
        private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

        public AbstractTenantSqlServerCustomFieldsStorageCreator(IDbContextProvider<TKey, TEntity> contextProvider, IEntityTableCreator tableCreator)
            : base(contextProvider, tableCreator)
        {
        }

        protected override async Task<string> GetCacheKey(CancellationToken cancellationToken)
        {
            var tenant = await _tenantEntityProvider.GetTenantAsync(cancellationToken).ConfigureAwait(false);

            return $"{tenant.TenantId}_{typeof(TEntity).Name}";
        }
    }
}
