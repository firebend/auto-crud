using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;

namespace Firebend.AutoCrud.Core.Interfaces.Services.CustomFields
{
    public interface ICustomFieldsCreateService<TKey, TEntity> : IDisposable
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        Task<CustomFieldsEntity<TKey>> CreateAsync(TKey rootEntityKey,
            CustomFieldsEntity<TKey> entity,
            CancellationToken cancellationToken = default);

        Task<CustomFieldsEntity<TKey>> CreateAsync(TKey rootEntityKey,
            CustomFieldsEntity<TKey> entity,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default);
    }
}
