using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Elastic.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.Data.SqlClient;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations.Abstractions;

public abstract class AbstractShardDbContextConnectionStringProvider<TKey, TEntity> : IDbContextConnectionStringProvider<TKey, TEntity>
    where TEntity : IEntity<TKey>
    where TKey : struct
{
    private readonly IShardKeyProvider _shardKeyProvider;
    private readonly IShardManager _shardManager;
    private readonly ShardMapMangerConfiguration _shardMapMangerConfiguration;
    private readonly IShardNameProvider _shardNameProvider;
    private readonly IMemoizer _memoizer;

    protected AbstractShardDbContextConnectionStringProvider(
        IShardManager shardManager,
        IShardKeyProvider shardKeyProvider,
        IShardNameProvider shardNameProvider,
        ShardMapMangerConfiguration shardMapMangerConfiguration,
        IMemoizer memoizer)
    {
        _shardManager = shardManager;
        _shardKeyProvider = shardKeyProvider;
        _shardNameProvider = shardNameProvider;
        _shardMapMangerConfiguration = shardMapMangerConfiguration;
        _memoizer = memoizer;
    }

    public async Task<string> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        var shardKey = _shardKeyProvider?.GetShardKey();

        if (string.IsNullOrWhiteSpace(shardKey))
        {
            throw new Exception("Shard key is null");
        }

        var memoizeKey = $"{shardKey}.Sharding.Enrollment";

        var connectionString = await _memoizer.MemoizeAsync<
            string,
            (AbstractShardDbContextConnectionStringProvider<TKey, TEntity> self, string shardKey, CancellationToken cancellationToken)>(
            memoizeKey,
            static arg => arg.self.GetShardConnectionStringAsync(arg.shardKey, arg.cancellationToken),
            (this, shardKey, cancellationToken),
            cancellationToken);

        return connectionString;
    }

    private async Task<string> GetShardConnectionStringAsync(string key, CancellationToken cancellationToken)
    {
        var shard = await _shardManager
            .RegisterShardAsync(_shardNameProvider?.GetShardName(key), key, cancellationToken)
            .ConfigureAwait(false);

        var keyBytes = Encoding.ASCII.GetBytes(key);

        var rootConnectionStringBuilder = new SqlConnectionStringBuilder(_shardMapMangerConfiguration.ConnectionString);
        rootConnectionStringBuilder.Remove("Data Source");
        rootConnectionStringBuilder.Remove("Initial Catalog");

        var shardConnectionString = rootConnectionStringBuilder.ConnectionString;

        await using var connection = await shard
            .OpenConnectionForKeyAsync(keyBytes, shardConnectionString)
            .ConfigureAwait(false);

        var connectionStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString) { Password = rootConnectionStringBuilder.Password };

        var connectionString = connectionStringBuilder.ConnectionString;

        return connectionString;
    }
}
