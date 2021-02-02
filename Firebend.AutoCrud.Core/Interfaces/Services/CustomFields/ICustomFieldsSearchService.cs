using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Interfaces.Services.CustomFields
{
    public interface ICustomFieldsSearchService<TKey, TEntity> : IDisposable
        where TKey : struct
        where TEntity : IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        Task<EntityPagedResponse<CustomFieldsEntity<TKey>>> SearchAsync(
            string key,
            string value,
            int? pageNumber,
            int? pageSize,
            CancellationToken cancellationToken = default);
    }
}
