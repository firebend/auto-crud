using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface IEntityUpdateService<in TKey, TEntity> : IDisposable
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    Task<TEntity> UpdateAsync(TEntity entity,
        CancellationToken cancellationToken);

    Task<TEntity> UpdateAsync(TEntity entity,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken );

    Task<TEntity> PatchAsync(TKey key,
        JsonPatchDocument<TEntity> jsonPatchDocument,
        CancellationToken cancellationToken);

    Task<TEntity> PatchAsync(TKey key,
        JsonPatchDocument<TEntity> jsonPatchDocument,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);
}
