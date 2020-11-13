using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoTenantReadClient<TKey, TEntity, TTenantKey> : MongoReadClient<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ITenantEntity<TTenantKey>
        where TTenantKey : struct
    {
        private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

        protected MongoTenantReadClient(IMongoClient client,
            ILogger<MongoReadClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            ITenantEntityProvider<TTenantKey> tenantEntityProvider) : base(client, logger, entityConfiguration)
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
