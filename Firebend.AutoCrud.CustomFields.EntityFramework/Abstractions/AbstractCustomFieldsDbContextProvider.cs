using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public abstract class AbstractCustomFieldsDbContextProvider<TKey, TEntity, TCustomFieldsEntity> : IDbContextProvider<Guid, TCustomFieldsEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
        where TCustomFieldsEntity : EfCustomFieldsModel<TKey, TEntity>
    {
        private readonly IDbContextProvider<TKey, TEntity> _rootProvider;

        protected AbstractCustomFieldsDbContextProvider(IDbContextProvider<TKey, TEntity> rootProvider)
        {
            _rootProvider = rootProvider;
        }

        public Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default)
            => _rootProvider.GetDbContextAsync(cancellationToken);
    }
}
