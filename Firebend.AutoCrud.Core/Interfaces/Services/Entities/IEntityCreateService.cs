using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface IEntityCreateService<TKey, TEntity> : IDisposable
    where TEntity : class, IEntity<TKey>
    where TKey : struct
{
    public Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken);

    public Task<TEntity> CreateAsync(TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken);
}
