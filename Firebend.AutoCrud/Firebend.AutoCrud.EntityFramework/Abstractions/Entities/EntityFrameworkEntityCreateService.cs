using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public abstract class EntityFrameworkEntityCreateService<TKey, TEntity> : IEntityCreateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        private readonly IEntityFrameworkCreateClient<TKey, TEntity> _createClient;

        protected EntityFrameworkEntityCreateService(IEntityFrameworkCreateClient<TKey, TEntity> createClient)
        {
            _createClient = createClient;
        }

        public virtual Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return _createClient.AddAsync(entity, cancellationToken);
        }
    }



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
            if(tenant != null)
            {
                entity.TenantId = tenant.TenantId;
            }
            return await base.CreateAsync(entity, cancellationToken);
        }
    }
}