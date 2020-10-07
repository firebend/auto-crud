using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoCollectionKeyGenerator<TEntity, TKey>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        Task<TKey> GenerateKeyAsync(CancellationToken cancellationToken = default);
    }
}