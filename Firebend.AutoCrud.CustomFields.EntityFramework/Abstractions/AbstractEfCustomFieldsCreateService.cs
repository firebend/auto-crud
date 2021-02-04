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
    public abstract class AbstractEfCustomFieldsCreateService<TKey, TEntity> : BaseDisposable, ICustomFieldsCreateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>, new()
    {
        private readonly IEntityFrameworkCreateClient<Guid, CustomFieldsEntity<TKey, TEntity>> _createClient;

        protected AbstractEfCustomFieldsCreateService(IEntityFrameworkCreateClient<Guid, CustomFieldsEntity<TKey, TEntity>> createClient)
        {
            _createClient = createClient;
        }

        public async Task<CustomFieldsEntity<TKey>> CreateAsync(TKey rootEntityKey, CustomFieldsEntity<TKey> entity, CancellationToken cancellationToken = default)
        {
            var customFieldsEntity = new CustomFieldsEntity<TKey, TEntity>(entity) {EntityId = rootEntityKey};
            var added = await _createClient.AddAsync(customFieldsEntity, cancellationToken).ConfigureAwait(false);

            if (added == null)
            {
                return null;
            }

            var returnEntity = CustomFieldsEntity<TKey>.Create(added);

            return returnEntity;
        }

        protected override void DisposeManagedObjects() => _createClient?.Dispose();
    }
}
