using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.EntityFramework.Interfaces;

public interface IEntityFrameworkUpdateClient<TKey, TEntity> : IDisposable
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    Task<TEntity> UpdateAsync(TEntity entity,
        CancellationToken cancellationToken);

    Task<TEntity> UpdateAsync(TEntity entity,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    Task<TEntity> UpdateAsync(TKey key,
        JsonPatchDocument<TEntity> patch,
        CancellationToken cancellationToken);

    Task<TEntity> UpdateAsync(TKey key,
        JsonPatchDocument<TEntity> patch,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);
}
