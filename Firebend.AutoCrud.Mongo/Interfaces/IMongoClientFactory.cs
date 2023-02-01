using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoClientFactory<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        Task<IMongoClient> CreateClientAsync(string overrideShardKey = null, bool enableLogging = false);
    }
}
