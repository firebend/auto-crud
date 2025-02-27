using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoClientFactory<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public Task<IMongoClient> CreateClientAsync(string overrideShardKey, CancellationToken cancellationToken);
}
