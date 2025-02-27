using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoConnectionStringProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public Task<string> GetConnectionStringAsync(string overrideShardKey, CancellationToken cancellationToken);
}
