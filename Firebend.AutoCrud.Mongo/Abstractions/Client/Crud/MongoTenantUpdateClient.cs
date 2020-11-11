using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Interfaces.Services.JsonPatch;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoTenantUpdateClient<TKey, TEntity, TTenantKey> : MongoUpdateClient<TKey, TEntity>,
        IMongoUpdateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ITenantEntity<TTenantKey>, new()
        where TTenantKey : struct

    {
        private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

        protected MongoTenantUpdateClient(IMongoClient client, ILogger<MongoUpdateClient<TKey, TEntity>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            IMongoCollectionKeyGenerator<TKey, TEntity> keyGenerator,
            IDomainEventContextProvider domainEventContextProvider,
            IJsonPatchDocumentGenerator jsonPatchDocumentGenerator,
            IEntityDomainEventPublisher domainEventPublisher,
            ITenantEntityProvider<TTenantKey> tenantEntityProvider)
            : base(client, logger, entityConfiguration, keyGenerator, domainEventContextProvider,
                jsonPatchDocumentGenerator, domainEventPublisher)
        {
            _tenantEntityProvider = tenantEntityProvider;
        }

        protected override async Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(
            CancellationToken cancellationToken)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            Expression<Func<TEntity, bool>> tenantFilter = x => x.TenantId.Equals(tenant.TenantId);
            return new[] { tenantFilter };
        }

        public override Task<TEntity> UpdateAsync(TKey id, JsonPatchDocument<TEntity> patch,
            CancellationToken cancellationToken = default)
        {
            patch?.Operations.RemoveAll(x => x.path == "/tenantId");
            return base.UpdateAsync(id, patch, cancellationToken);
        }

        protected override async Task<TEntity> UpdateInternalAsync(TEntity entity,
            Expression<Func<TEntity, bool>> filter, bool doUpsert, CancellationToken cancellationToken,
            JsonPatchDocument<TEntity> patchDocument = null)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            entity.TenantId = tenant.TenantId;
            patchDocument?.Operations.RemoveAll(x => x.path == "/tenantId");
            return await base.UpdateInternalAsync(entity, filter, doUpsert, cancellationToken, patchDocument).ConfigureAwait(false);
        }
    }
}
