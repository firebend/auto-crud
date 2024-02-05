using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations;

public class ShardEntityFrameworkMigrationsConnectionStringProvider : IEntityFrameworkMigrationsConnectionStringProvider
{
    private readonly IAllShardKeyProvider _shardKeyProvider;
    private readonly IShardKeyConnectionStringProvider _shardKeyConnectionStringProvider;

    public ShardEntityFrameworkMigrationsConnectionStringProvider(IAllShardKeyProvider shardKeyProvider,
        IShardKeyConnectionStringProvider shardKeyConnectionStringProvider)
    {
        _shardKeyProvider = shardKeyProvider;
        _shardKeyConnectionStringProvider = shardKeyConnectionStringProvider;
    }

    public async Task<string[]> GetConnectionStringsAsync(CancellationToken cancellationToken)
    {
        var shards = await _shardKeyProvider.GetAllShards(cancellationToken);

        var connections = new string[shards.Length];

        for (var index = 0; index < shards.Length; index++)
        {
            var connection = await _shardKeyConnectionStringProvider.GetShardConnectionStringAsync(shards[index], cancellationToken);
            connections[index] = connection;
        }

        return connections;
    }
}
