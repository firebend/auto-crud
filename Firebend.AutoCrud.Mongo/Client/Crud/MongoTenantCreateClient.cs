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

namespace Firebend.AutoCrud.Mongo.Client.Crud;

public class MongoTenantCreateClient<TKey, TEntity, TTenantKey> : MongoCreateClient<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ITenantEntity<TTenantKey>, new()
    where TTenantKey : struct
{
    private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

    public MongoTenantCreateClient(IMongoClientFactory<TKey, TEntity> clientFactory,
        ILogger<MongoTenantCreateClient<TKey, TEntity, TTenantKey>> logger,
        IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
        IMongoRetryService mongoRetryService,
        ITenantEntityProvider<TTenantKey> tenantEntityProvider,
        IMongoReadPreferenceService readPreferenceService,
        IDomainEventPublisherService<TKey, TEntity> publisherService = null)
        : base(clientFactory, logger, entityConfiguration, mongoRetryService, readPreferenceService, publisherService)
    {
        _tenantEntityProvider = tenantEntityProvider;
    }

    protected override async Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken)
    {
        var tenant = await _tenantEntityProvider.GetTenantAsync(cancellationToken);

        Expression<Func<TEntity, bool>> tenantFilter = x => x.TenantId.Equals(tenant.TenantId);

        return [tenantFilter];
    }

    protected override async Task<TEntity> CreateInternalAsync(TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken)
    {
        var tenant = await _tenantEntityProvider.GetTenantAsync(cancellationToken);

        entity.TenantId = tenant.TenantId;

        return await base.CreateInternalAsync(entity, transaction, cancellationToken);
    }
}
