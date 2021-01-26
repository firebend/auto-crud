using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public abstract class EntityFrameworkTenantQueryClient<TKey, TEntity, TTenantKey> : EntityFrameworkQueryClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ITenantEntity<TTenantKey>, new()
        where TTenantKey : struct
    {
        private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

        protected EntityFrameworkTenantQueryClient(IDbContextProvider<TKey, TEntity> contextProvider,
            ITenantEntityProvider<TTenantKey> tenantEntityProvider,
            IEntityQueryOrderByHandler<TKey, TEntity> queryOrderByHandler,
            IEntityFrameworkIncludesProvider<TKey, TEntity> includesProvider) :
            base(contextProvider, queryOrderByHandler, includesProvider)
        {
            _tenantEntityProvider = tenantEntityProvider;
        }

        protected override async Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantEntityProvider
                .GetTenantAsync(cancellationToken)
                .ConfigureAwait(false);

            Expression<Func<TEntity, bool>> filter = x => x.TenantId.Equals(tenant.TenantId);

            return new[] { filter };
        }
    }
}
