using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions
{
    public class AbstractEfCustomFieldsDeleteService<TKey, TEntity> : BaseDisposable, ICustomFieldsDeleteService<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        private readonly IEntityFrameworkDeleteClient<Guid, CustomFieldsEntity<TKey, TEntity>> _deleteClient;

        public AbstractEfCustomFieldsDeleteService(IEntityFrameworkDeleteClient<Guid, CustomFieldsEntity<TKey, TEntity>> deleteClient)
        {
            _deleteClient = deleteClient;
        }

        public async Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey, Guid key, CancellationToken cancellationToken = default)
        {
            var deleted = await _deleteClient
                .DeleteAsync(key, cancellationToken)
                .ConfigureAwait(false);

            if (deleted == null)
            {
                return null;
            }

            var retDeleted = CustomFieldsEntity<TKey>.Create(deleted);

            return retDeleted;
        }

        protected override void DisposeManagedObjects() => _deleteClient.Dispose();
    }
}
