using Firebend.AutoCrud.ChangeTracking.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Abstractions;

public class AbstractElasticChangeTrackingDbContextProvider<TEntityKey, TEntity, TContext>
    : AbstractChangeTrackingDbContextProvider<TEntityKey, TEntity, TContext>
    where TEntityKey : struct
    where TEntity : class, IEntity<TEntityKey>
    where TContext : DbContext, IDbContext
{

    public AbstractElasticChangeTrackingDbContextProvider(
        IDbContextOptionsProvider<TEntityKey, TEntity> optionsProvider,
        IDbContextConnectionStringProvider<TEntityKey, TEntity> connectionStringProvider,
        IChangeTrackingOptionsProvider<TEntityKey, TEntity> changeTrackingOptionsProvider) : base(optionsProvider, connectionStringProvider, changeTrackingOptionsProvider)
    {
    }
}
