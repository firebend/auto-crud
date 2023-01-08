using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoConnectionStringProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    Task<string> GetConnectionStringAsync(CancellationToken cancellationToken = default);
}
