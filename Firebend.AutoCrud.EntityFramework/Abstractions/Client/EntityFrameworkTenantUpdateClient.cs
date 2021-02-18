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

        public override Task<TEntity> UpdateAsync(TKey key, JsonPatchDocument<TEntity> jsonPatchDocument, CancellationToken cancellationToken = default)
        {
            if (jsonPatchDocument != null)
            {
                jsonPatchDocument.Operations.RemoveAll(x => x.path == "/tenantId");
            }

            return base.UpdateAsync(key, jsonPatchDocument, cancellationToken);
        }

        public override async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            entity.TenantId = tenant.TenantId;

            return await base.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
        }
    }
}
