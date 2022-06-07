using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.JsonPatch;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public abstract class EntityFrameworkTenantUpdateClient<TKey, TEntity, TTenantKey> : EntityFrameworkUpdateClient<TKey, TEntity>
        where TKey : struct
        where TTenantKey : struct
        where TEntity : class, IEntity<TKey>, ITenantEntity<TTenantKey>, new()
    {
        private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

        protected EntityFrameworkTenantUpdateClient(IDbContextProvider<TKey, TEntity> contextProvider,
            IEntityDomainEventPublisher domainEventPublisher,
            IDomainEventContextProvider domainEventContextProvider,
            IJsonPatchGenerator jsonPatchDocumentGenerator,
            ITenantEntityProvider<TTenantKey> tenantEntityProvider,
            IEntityFrameworkDbUpdateExceptionHandler<TKey, TEntity> exceptionHandler) : base(
            contextProvider,
            domainEventPublisher,
            domainEventContextProvider,
            jsonPatchDocumentGenerator,
            exceptionHandler)
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

        protected virtual JsonPatchDocument<TEntity> RemoveTenantId(JsonPatchDocument<TEntity> jsonPatchDocument)
        {
            jsonPatchDocument?.Operations.RemoveAll(x => x.path == "/tenantId");

            return jsonPatchDocument;
        }

        protected virtual async Task<TEntity> SetTenantAsync(TEntity entity, CancellationToken cancellationToken)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            entity.TenantId = tenant.TenantId;

            return entity;
        }

        public override Task<TEntity> UpdateAsync(TKey key,
            JsonPatchDocument<TEntity> jsonPatchDocument,
            CancellationToken cancellationToken = default)
        {
            jsonPatchDocument = RemoveTenantId(jsonPatchDocument);

            return base.UpdateAsync(key, jsonPatchDocument, cancellationToken);
        }

        public override Task<TEntity> UpdateAsync(TKey key,
            JsonPatchDocument<TEntity> jsonPatchDocument,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
        {
            jsonPatchDocument = RemoveTenantId(jsonPatchDocument);

            return base.UpdateAsync(key, jsonPatchDocument, entityTransaction, cancellationToken);
        }

        public override async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            entity = await SetTenantAsync(entity, cancellationToken);

            return await base.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        public override async Task<TEntity> UpdateAsync(TEntity entity, IEntityTransaction entityTransaction, CancellationToken cancellationToken = default)
        {
            entity = await SetTenantAsync(entity, cancellationToken);

            return await base.UpdateAsync(entity, entityTransaction, cancellationToken).ConfigureAwait(false);
        }
    }
}
