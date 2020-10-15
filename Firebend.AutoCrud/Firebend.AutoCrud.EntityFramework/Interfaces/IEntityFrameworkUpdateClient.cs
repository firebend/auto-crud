#region

using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.JsonPatch;

#endregion

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IEntityFrameworkUpdateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<TEntity> UpdateAsync(TKey key, JsonPatchDocument<TEntity> patch, CancellationToken cancellationToken = default);
    }
}