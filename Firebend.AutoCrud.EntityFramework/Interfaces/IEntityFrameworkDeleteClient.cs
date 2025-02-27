using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.EntityFramework.Interfaces;

public interface IEntityFrameworkDeleteClient<TKey, TEntity> : IDisposable
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    public Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken);

    public Task<TEntity> DeleteAsync(TKey filter, IEntityTransaction entityTransaction, CancellationToken cancellationToken);

    public Task<IEnumerable<TEntity>> DeleteAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken);

    public Task<IEnumerable<TEntity>> DeleteAsync(Expression<Func<TEntity, bool>> filter, IEntityTransaction entityTransaction, CancellationToken cancellationToken);
}
