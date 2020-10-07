using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntityUpdateService<in TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<TEntity> PatchAsync(TKey key, JsonPatchDocument<TEntity> jsonPatchDocument, CancellationToken cancellationToken = default);
    }
}