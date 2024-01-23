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

namespace Firebend.AutoCrud.Mongo.Abstractions.Client.Crud;

public abstract class MongoTenantDeleteClient<TKey, TEntity, TTenantKey> : MongoDeleteClient<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ITenantEntity<TTenantKey>
    where TTenantKey : struct
{
    private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

    protected MongoTenantDeleteClient(IMongoClientFactory<TKey, TEntity> clientFactory,
        ILogger<MongoTenantDeleteClient<TKey, TEntity, TTenantKey>> logger,
        IMongoEntityConfiguration<TKey, TEntity> entityConfiguration,
        ITenantEntityProvider<TTenantKey> tenantEntityProvider,
        IMongoRetryService mongoRetryService,
        IDomainEventPublisherService<TKey, TEntity> publisherService = null) : base(clientFactory, logger, entityConfiguration, mongoRetryService, publisherService)
    {
        _tenantEntityProvider = tenantEntityProvider;
    }

    protected override async Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken)
    {
        var tenant = await _tenantEntityProvider
            .GetTenantAsync(cancellationToken)
            .ConfigureAwait(false);

        Expression<Func<TEntity, bool>> tenantFilter = x => x.TenantId.Equals(tenant.TenantId);

        return new[]
        {
            tenantFilter
        };
    }
}
