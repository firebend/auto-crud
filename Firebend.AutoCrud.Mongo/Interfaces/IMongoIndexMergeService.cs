using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoIndexMergeService
    {
        Task MergeIndexesAsync<TEntity>(IMongoCollection<TEntity> collection,
            CreateIndexModel<TEntity>[] indexModels,
            CancellationToken cancellationToken);
    }
}
