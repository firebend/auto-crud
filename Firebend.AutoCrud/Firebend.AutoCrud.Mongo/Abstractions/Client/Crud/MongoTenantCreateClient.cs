using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoTenantCreateClient<TKey, TEntity, TTenantKey> : MongoCreateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ITenantEntity<TTenantKey>, new()
        where TTenantKey: struct
    {
        private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

        protected MongoTenantCreateClient(IMongoClient client, ILogger<MongoCreateClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration, IEntityDomainEventPublisher eventPublisher,
            IDomainEventContextProvider domainEventContextProvider, 
            ITenantEntityProvider<TTenantKey> tenantEntityProvider) : base(client, logger, entityConfiguration,
            eventPublisher, domainEventContextProvider)
        {
            _tenantEntityProvider = tenantEntityProvider;
        }

        protected override async Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            Expression<Func<TEntity, bool>> tenantFilter = x => x.TenantId.Equals(tenant.TenantId);

            return new[] {tenantFilter};
        }

        public override async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            entity.TenantId = tenant.TenantId;
            return await base.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
        }
    }
}