using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface IEntityReadService<in TKey, TEntity> : IDisposable
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public Task<TEntity> GetByKeyAsync(TKey key,
        CancellationToken cancellationToken);

    public Task<TEntity> GetByKeyAsync(TKey key,
        IEntityTransaction transaction,
        CancellationToken cancellationToken);

    public Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);

    public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken);

    public Task<List<TEntity>> GetAllAsync(IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken);

    public Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction transaction,
        CancellationToken cancellationToken);

    public Task<TEntity> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken);

    public Task<TEntity> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);
}
