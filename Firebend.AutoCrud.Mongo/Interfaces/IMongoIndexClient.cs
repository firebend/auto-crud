using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces;

// ReSharper disable once UnusedTypeParameter
public interface IMongoIndexClient<TKey, TEntity>
    where TEntity : IEntity<TKey>
    where TKey : struct
{
    Task BuildIndexesAsync(IMongoEntityIndexConfiguration<TKey, TEntity> configuration, CancellationToken cancellationToken);

    Task CreateCollectionAsync(IMongoEntityIndexConfiguration<TKey, TEntity> configuration, CancellationToken cancellationToken);
}
