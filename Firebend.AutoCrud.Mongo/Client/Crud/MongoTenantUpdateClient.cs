using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Mongo.Client.Crud;

public class MongoTenantUpdateClient<TKey, TEntity, TTenantKey> : MongoUpdateClient<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ITenantEntity<TTenantKey>, new()
    where TTenantKey : struct

{
    private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

    public MongoTenantUpdateClient(IMongoClientFactory<TKey, TEntity> clientFactory,
        ILogger<MongoTenantUpdateClient<TKey, TEntity, TTenantKey>> logger,
        IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
        IMongoCollectionKeyGenerator<TKey, TEntity> keyGenerator,
        IMongoRetryService retryService,
        ITenantEntityProvider<TTenantKey> tenantEntityProvider,
        IMongoReadPreferenceService readPreferenceService,
        IDomainEventPublisherService<TKey, TEntity> domainEventPublisher = null) : base(
            clientFactory,
            logger,
            entityConfiguration,
            keyGenerator,
            retryService,
            readPreferenceService,
            domainEventPublisher)
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
        CancellationToken cancellationToken)
        => UpdateAsync(id, patch, null, cancellationToken);

    public override Task<TEntity> UpdateAsync(TKey id,
        JsonPatchDocument<TEntity> patch,
        IEntityTransaction transaction,
        CancellationToken cancellationToken)
    {
        patch = RemoveTenantId(patch);
        return base.UpdateAsync(id, patch, transaction, cancellationToken);
    }

    protected override async Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(
        CancellationToken cancellationToken)
    {
        var tenant = await _tenantEntityProvider.GetTenantAsync(cancellationToken);

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
        var tenant = await _tenantEntityProvider.GetTenantAsync(cancellationToken);

        entity.TenantId = tenant.TenantId;
        patchDocument = RemoveTenantId(patchDocument);

        return await base.UpdateInternalAsync(entity, filter, doUpsert, entityTransaction, patchDocument, original, cancellationToken);
    }
}
