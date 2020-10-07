using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services
{
    public interface IEntitySearchService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        Task<List<TEntity>> SearchAsync(EntitySearchRequest request,  CancellationToken cancellationToken = default);

        Task<EntityPagedResponse<TEntity, TKey>> PageAsync(EntitySearchRequest request, CancellationToken cancellationToken = default);
    }
}