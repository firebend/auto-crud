using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface IEntityDeleteService<in TKey, TEntity> : IDisposable
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken);
    Task<TEntity> DeleteAsync(TKey key, IEntityTransaction entityTransaction, CancellationToken cancellationToken);
}
