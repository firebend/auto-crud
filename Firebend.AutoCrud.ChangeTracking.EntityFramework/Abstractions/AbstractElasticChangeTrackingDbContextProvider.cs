using System;
using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Abstractions;

public class AbstractElasticChangeTrackingDbContextProvider<TEntityKey, TEntity, TContext>
    : AbstractChangeTrackingDbContextProvider<TEntityKey, TEntity, TContext>
    where TEntityKey : struct
    where TEntity : class, IEntity<TEntityKey>
    where TContext : DbContext, IDbContext
{
    private readonly IShardNameProvider _shardNameProvider;
    private readonly IShardKeyProvider _shardKeyProvider;

    public AbstractElasticChangeTrackingDbContextProvider(
        IDbContextOptionsProvider<TEntityKey, TEntity> optionsProvider,
        IDbContextConnectionStringProvider<TEntityKey, TEntity> connectionStringProvider,
        IChangeTrackingOptionsProvider<TEntityKey, TEntity> changeTrackingOptionsProvider,
        IMemoizer memoizer,
        IShardNameProvider shardNameProvider,
        IShardKeyProvider shardKeyProvider) : base(optionsProvider, connectionStringProvider, changeTrackingOptionsProvider, memoizer)
    {
        _shardNameProvider = shardNameProvider;
        _shardKeyProvider = shardKeyProvider;
    }

    private string _scaffoldKey;

    protected override string GetScaffoldingKey(Type type)
    {
        _scaffoldKey ??= $"{type.FullName}.{_shardNameProvider.GetShardName(_shardKeyProvider.GetShardKey())}.Changes.Scaffolding";
        return _scaffoldKey;
    }
}
