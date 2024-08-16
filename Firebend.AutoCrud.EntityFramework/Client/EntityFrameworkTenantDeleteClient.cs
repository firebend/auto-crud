using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Client;

public class EntityFrameworkTenantDeleteClient<TKey, TEntity, TTenantKey> : EntityFrameworkDeleteClient<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>, ITenantEntity<TTenantKey>, new()
    where TTenantKey : struct
{
    private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

    public EntityFrameworkTenantDeleteClient(IDbContextProvider<TKey, TEntity> contextProvider,
        IEntityReadService<TKey, TEntity> readService,
        ITenantEntityProvider<TTenantKey> tenantEntityProvider,
        IDomainEventPublisherService<TKey, TEntity> publisherService = null) : base(contextProvider, readService, publisherService)
    {
        _tenantEntityProvider = tenantEntityProvider;
    }


    protected override async Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken)
    {
        var tenant = await _tenantEntityProvider.GetTenantAsync(cancellationToken);

        Expression<Func<TEntity, bool>> tenantFilter = x => x.TenantId.Equals(tenant.TenantId);

        return new[] { tenantFilter };
    }
}
