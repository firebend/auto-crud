using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Elastic.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.Data.SqlClient;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations;

public class ShardDbContextConnectionStringProvider<TKey, TEntity> :
    IDbContextConnectionStringProvider<TKey, TEntity>,
    IShardKeyConnectionStringProvider
    where TEntity : IEntity<TKey>
    where TKey : struct
{

    private readonly IShardKeyProvider _shardKeyProvider;
    private readonly IShardManager _shardManager;
    private readonly ShardMapMangerConfiguration _shardMapMangerConfiguration;
    private readonly IShardNameProvider _shardNameProvider;

    public ShardDbContextConnectionStringProvider(
        IShardManager shardManager,
        IShardKeyProvider shardKeyProvider,
        IShardNameProvider shardNameProvider,
        ShardMapMangerConfiguration shardMapMangerConfiguration)
    {
        _shardManager = shardManager;
        _shardKeyProvider = shardKeyProvider;
        _shardNameProvider = shardNameProvider;
        _shardMapMangerConfiguration = shardMapMangerConfiguration;
    }

    public async Task<string> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        var shardKey = await _shardKeyProvider.GetShardKeyAsync(cancellationToken);
        var connection = await GetShardConnectionStringAsync(shardKey, cancellationToken);
        return connection;
    }

    private static async Task<string> GetShardConnectionStringAsync(string shardKey, ConnectionStringContext ctx)
    {
        var shard = await ctx.ShardManager.RegisterShardAsync(ctx.ShardNameProvider?.GetShardName(ctx.Key), ctx.Key, ctx.CancellationToken);

        var keyBytes = Encoding.ASCII.GetBytes(shardKey);

        var rootConnectionStringBuilder = new SqlConnectionStringBuilder(ctx.ShardMapMangerConfiguration.ConnectionString);
        rootConnectionStringBuilder.Remove("Data Source");
        rootConnectionStringBuilder.Remove("Initial Catalog");

        var shardConnectionString = rootConnectionStringBuilder.ConnectionString;

        await using var connection = await shard.OpenConnectionForKeyAsync(keyBytes, shardConnectionString);

        var connectionStringBuilder = new SqlConnectionStringBuilder(connection.ConnectionString) { Password = rootConnectionStringBuilder.Password };

        var connectionString = connectionStringBuilder.ConnectionString;

        return connectionString;
    }

    public async Task<string> GetShardConnectionStringAsync(string shardKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(shardKey))
        {
            throw new Exception("Shard key is null");
        }

        var context = new ConnectionStringContext(shardKey, _shardManager, _shardMapMangerConfiguration, _shardNameProvider, cancellationToken);

        var connectionString = await ConnectionStringProviderCache.ConnectionStringCache.GetOrAdd(shardKey, GetShardConnectionStringAsync, context);

        return connectionString;
    }
}
