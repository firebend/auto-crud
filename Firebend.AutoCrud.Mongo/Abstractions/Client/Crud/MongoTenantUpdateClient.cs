using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.JsonPatch;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud
{
    public abstract class MongoTenantUpdateClient<TKey, TEntity, TTenantKey> : MongoUpdateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ITenantEntity<TTenantKey>, new()
        where TTenantKey : struct

    {
        private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

        protected MongoTenantUpdateClient(IMongoClient client,
            ILogger<MongoTenantUpdateClient<TKey, TEntity, TTenantKey>> logger,
            IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
            IMongoCollectionKeyGenerator<TKey, TEntity> keyGenerator,
            IDomainEventContextProvider domainEventContextProvider,
            IJsonPatchGenerator jsonPatchDocumentGenerator,
            IEntityDomainEventPublisher domainEventPublisher,
            ITenantEntityProvider<TTenantKey> tenantEntityProvider,
            IMongoRetryService retryService)
            : base(client, logger, entityConfiguration, keyGenerator, domainEventContextProvider,
                jsonPatchDocumentGenerator, domainEventPublisher, retryService)
        {
            _tenantEntityProvider = tenantEntityProvider;
        }


        private static JsonPatchDocument<TEntity> RemoveTenantId(JsonPatchDocument<TEntity> jsonPatchDocument)
        {
            jsonPatchDocument?.Operations.RemoveAll(x => x.path == "/tenantId");

            return jsonPatchDocument;
        }

        public override Task<TEntity> UpdateAsync(TKey id,
            JsonPatchDocument<TEntity> patch,
            CancellationToken cancellationToken = default)
            => UpdateAsync(id, patch, null, cancellationToken);

        public override Task<TEntity> UpdateAsync(TKey id,
            JsonPatchDocument<TEntity> patch,
            IEntityTransaction transaction,
            CancellationToken cancellationToken = default)
        {
            patch = RemoveTenantId(patch);
            return base.UpdateAsync(id, patch, transaction, cancellationToken);
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


        protected override async Task<TEntity> UpdateInternalAsync(TEntity entity,
            Expression<Func<TEntity, bool>> filter,
            bool doUpsert,
            IEntityTransaction entityTransaction,
            JsonPatchDocument<TEntity> patchDocument,
            TEntity original,
            CancellationToken cancellationToken)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            entity.TenantId = tenant.TenantId;
            patchDocument = RemoveTenantId(patchDocument);

            return await base
                .UpdateInternalAsync(entity, filter, doUpsert, entityTransaction, patchDocument, original, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
