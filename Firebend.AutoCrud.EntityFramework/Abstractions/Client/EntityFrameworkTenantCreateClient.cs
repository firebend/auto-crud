using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public abstract class EntityFrameworkTenantCreateClient<TKey, TEntity, TTenantKey> : EntityFrameworkCreateClient<TKey, TEntity>
        where TKey : struct
        where TTenantKey : struct
        where TEntity : class, IEntity<TKey>, ITenantEntity<TTenantKey>, new()
    {
        private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

        protected EntityFrameworkTenantCreateClient(IDbContextProvider<TKey, TEntity> provider,
            IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity> exceptionHandler,
            IDomainEventPublisherService<TKey, TEntity> publisherService,
            ITenantEntityProvider<TTenantKey> tenantEntityProvider) : base(provider, exceptionHandler, publisherService)
        {
            _tenantEntityProvider = tenantEntityProvider;
        }

        protected override async Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            Expression<Func<TEntity, bool>> tenantFilter = x => x.TenantId.Equals(tenant.TenantId);

            return new[] { tenantFilter };
        }

        public override async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            entity.TenantId = tenant.TenantId;

            return await base.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<TEntity> AddAsync(TEntity entity, IEntityTransaction entityTransaction, CancellationToken cancellationToken)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            entity.TenantId = tenant.TenantId;

            return await base.AddAsync(entity, entityTransaction, cancellationToken).ConfigureAwait(false);
        }
    }
}
