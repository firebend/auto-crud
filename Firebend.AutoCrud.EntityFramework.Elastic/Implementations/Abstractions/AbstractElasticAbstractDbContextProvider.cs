using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations.Abstractions;

public class AbstractElasticDbContextProvider<TKey, TEntity, TContext> : AbstractDbContextProvider<TKey, TEntity, TContext>
    where TKey : struct
    where TEntity : IEntity<TKey>
    where TContext : DbContext, IDbContext
{
    private readonly IShardNameProvider _shardNameProvider;
    private readonly IShardKeyProvider _shardKeyProvider;

    public AbstractElasticDbContextProvider(
        IDbContextConnectionStringProvider<TKey, TEntity> connectionStringProvider,
        IDbContextOptionsProvider<TKey, TEntity> optionsProvider,
        ILoggerFactory loggerFactory, IMemoizer memoizer,
        IShardNameProvider shardNameProvider,
        IShardKeyProvider shardKeyProvider) : base(connectionStringProvider, optionsProvider, loggerFactory, memoizer)
    {
        _shardNameProvider = shardNameProvider;
        _shardKeyProvider = shardKeyProvider;
    }

    private string _memoizeKey;
    protected override string GetMemoizeKey(Type dbContextType)
        => _memoizeKey ??= $"{dbContextType.FullName}.{_shardNameProvider.GetShardName(_shardKeyProvider.GetShardKey())}.Init";

    private string _poolKey;
    protected override string GetPooledKey(Type dbContextType)
        => _poolKey ??= $"{dbContextType.FullName}.{_shardNameProvider.GetShardName(_shardKeyProvider.GetShardKey())}.Pooled";
}
