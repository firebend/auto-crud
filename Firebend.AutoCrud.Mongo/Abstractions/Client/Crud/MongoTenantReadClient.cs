using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoTenantReadClient<TKey, TEntity, TTenantKey> : MongoReadClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ITenantEntity<TTenantKey>
        where TTenantKey : struct
    {
        private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

        protected MongoTenantReadClient(IMongoClientFactory<TKey, TEntity> clientFactory,
            ILogger<MongoTenantReadClient<TKey, TEntity, TTenantKey>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            ITenantEntityProvider<TTenantKey> tenantEntityProvider,
            IEntityQueryOrderByHandler<TKey, TEntity> entityQueryOrderByHandler,
            IMongoRetryService retryService) : base(clientFactory, logger, entityConfiguration, entityQueryOrderByHandler, retryService)
        {
            _tenantEntityProvider = tenantEntityProvider;
        }

        protected override async Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            Expression<Func<TEntity, bool>> tenantFilter = x => x.TenantId.Equals(tenant.TenantId);

            return new[] { tenantFilter };
        }
    }
}
