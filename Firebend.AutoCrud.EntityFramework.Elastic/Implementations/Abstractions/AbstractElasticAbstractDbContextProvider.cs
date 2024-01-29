using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations.Abstractions;

public class AbstractElasticDbContextProvider<TKey, TEntity, TContext> : AbstractDbContextProvider<TKey, TEntity, TContext>
    where TKey : struct
    where TEntity : IEntity<TKey>
    where TContext : DbContext, IDbContext
{
    public AbstractElasticDbContextProvider(
        IDbContextConnectionStringProvider<TKey, TEntity> connectionStringProvider,
        IDbContextOptionsProvider<TKey, TEntity> optionsProvider,
        ILogger<AbstractElasticDbContextProvider<TKey, TEntity, TContext>> logger) : base(connectionStringProvider, optionsProvider, logger)
    {
    }
}
