using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public abstract class AbstractCustomFieldsConnectionStringProvider<TKey, TEntity, TCustomFieldsEntity> : IDbContextConnectionStringProvider<Guid, TCustomFieldsEntity>
        where TCustomFieldsEntity : IEntity<Guid>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IDbContextConnectionStringProvider<TKey, TEntity> _connectionStringProvider;

        protected AbstractCustomFieldsConnectionStringProvider(IDbContextConnectionStringProvider<TKey, TEntity> connectionStringProvider)
        {
            _connectionStringProvider = connectionStringProvider;
        }

        public Task<string> GetConnectionStringAsync(CancellationToken cancellationToken = default) =>
            _connectionStringProvider.GetConnectionStringAsync(cancellationToken);
    }
}
