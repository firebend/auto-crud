using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoIndexClient<TEntity, TKey>
        where TEntity : IEntity<TKey>
        where TKey : struct
    {
        
        Task BuildIndexesAsync(CancellationToken cancellationToken = default);

        Task CreateCollectionAsync(CancellationToken cancellationToken = default);
    }
}