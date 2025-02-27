using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.EntityFramework.Interfaces;

public interface IEntityFrameworkCreateClient<TKey, TEntity> : IDisposable
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);

    public Task<TEntity> AddAsync(TEntity entity, IEntityTransaction transaction, CancellationToken cancellationToken);
}
