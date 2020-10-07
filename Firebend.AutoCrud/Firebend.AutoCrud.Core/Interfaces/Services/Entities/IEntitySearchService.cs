using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntitySearchService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        Task<List<TEntity>> SearchAsync(EntitySearchRequest request,  CancellationToken cancellationToken = default);

        Task<EntityPagedResponse<TEntity, TKey>> PageAsync(EntitySearchRequest request, CancellationToken cancellationToken = default);
    }
}