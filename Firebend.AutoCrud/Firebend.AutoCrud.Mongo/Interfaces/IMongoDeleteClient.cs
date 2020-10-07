using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoDeleteClient<TEntity, TKey>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        Task<TEntity> DeleteAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    }
}