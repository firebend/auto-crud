using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntitySearchService<TKey, TEntity, TSearch>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TSearch : EntitySearchRequest
    {
        Task<List<TEntity>> SearchAsync(TSearch request,  CancellationToken cancellationToken = default);

        Task<EntityPagedResponse<TEntity>> PageAsync(TSearch request, CancellationToken cancellationToken = default);
    }
}