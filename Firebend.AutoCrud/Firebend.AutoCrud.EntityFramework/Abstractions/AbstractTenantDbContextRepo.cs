using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions
{
    public abstract class AbstractTenantDbContextRepo<TKey, TEntity, TTenantKey> : AbstractDbContextRepo<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ITenantEntity<TKey>, new()
        where TTenantKey : struct
    {
        private readonly ITenantEntityProvider<TTenantKey> _tenantEntityProvider;

        public AbstractTenantDbContextRepo(IDbContextProvider<TKey, TEntity> dbContextProvider, ITenantEntityProvider<TTenantKey> tenantEntityProvider) : base(dbContextProvider)
        {
            _tenantEntityProvider = tenantEntityProvider;
        }

        protected override async Task<IEnumerable<Expression<Func<TEntity, bool>>>> GetSecurityFiltersAsync(CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantEntityProvider.GetTenantAsync(cancellationToken);
            return new Expression<Func<TEntity, bool>>[] {
                x => x.TenantId.Equals(tenant.TenantId)
            };
        }
    }
}
