using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Implementations;

public class StaticMongoConnectionStringProvider<TKey, TEntity> : IMongoConnectionStringProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly string _connectionString;

    public StaticMongoConnectionStringProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<string> GetConnectionStringAsync(string overrideShardKey, CancellationToken cancellationToken)
        => Task.FromResult(_connectionString);
}
