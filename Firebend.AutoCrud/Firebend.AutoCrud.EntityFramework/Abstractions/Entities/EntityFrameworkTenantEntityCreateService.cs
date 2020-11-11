using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public abstract class EntityFrameworkTenantEntityCreateService<TKey, TEntity, TTenantKey> : EntityFrameworkEntityCreateService<TKey, TEntity>
       where TKey : struct
       where TEntity : class, IEntity<TKey>, ITenantEntity<TTenantKey>, new()
       where TTenantKey : struct
    {
        private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

        protected EntityFrameworkTenantEntityCreateService(IEntityFrameworkCreateClient<TKey, TEntity> createClient, ITenantEntityProvider<TTenantKey> tenantEntityProvider) : base(createClient)
        {
            _tenantEntityProvider = tenantEntityProvider;
        }

        public override async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantEntityProvider.GetTenantAsync(cancellationToken);
            if (tenant != null)
            {
                entity.TenantId = tenant.TenantId;
            }
            return await base.CreateAsync(entity, cancellationToken);
        }
    }
}
