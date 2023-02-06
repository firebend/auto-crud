using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoIndexMergeService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        Task MergeIndexesAsync(IMongoCollection<TEntity> collection,
            CreateIndexModel<TEntity>[] indexModels,
            CancellationToken cancellationToken);
    }
}
