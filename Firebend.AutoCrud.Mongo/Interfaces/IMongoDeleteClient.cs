using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoDeleteClient<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    public Task<TEntity> DeleteAsync(Expression<Func<TEntity, bool>> filter,
        CancellationToken cancellationToken);

    public Task<TEntity> DeleteAsync(Expression<Func<TEntity, bool>> filter,
        IEntityTransaction entityTransaction,
        CancellationToken cancellationToken);
}
