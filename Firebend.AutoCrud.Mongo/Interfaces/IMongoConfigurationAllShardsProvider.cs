using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoConfigurationAllShardsProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        Task<IEnumerable<IMongoEntityConfiguration<TKey, TEntity>>> GetAllEntityConfigurationsAsync(CancellationToken cancellationToken);
    }
}
