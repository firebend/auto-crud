using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    // ReSharper disable once UnusedTypeParameter
    public interface IMongoIndexClient<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        Task BuildIndexesAsync(IMongoEntityConfiguration<TKey, TEntity> configuration,  CancellationToken cancellationToken = default);

        Task CreateCollectionAsync(IMongoEntityConfiguration<TKey, TEntity> configuration,  CancellationToken cancellationToken = default);
    }
}
