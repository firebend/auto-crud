using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoClientFactory<TKey, TEntity>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        Task<IMongoClient> CreateClientAsync(bool enableLogging = false);
    }
}
