using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoCollectionKeyGenerator<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        Task<TKey> GenerateKeyAsync(CancellationToken cancellationToken = default);
    }
}