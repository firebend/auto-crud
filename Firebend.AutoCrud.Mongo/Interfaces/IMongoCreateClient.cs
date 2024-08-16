using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoCreateClient<TKey, TEntity>
    where TEntity : IEntity<TKey>
    where TKey : struct
{
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken);

    Task<TEntity> CreateAsync(TEntity entity, IEntityTransaction entityTransaction, CancellationToken cancellationToken);
}
