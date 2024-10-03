using System;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Implementations;

public class ChangeTrackingTableNameProvider<TEntityKey, TEntity, TEntityContext>(
    IDbContextFactory<TEntityContext> contextFactory) : IChangeTrackingTableNameProvider<TEntityKey, TEntity>
    where TEntity : class, IEntity<TEntityKey>
    where TEntityKey : struct
    where TEntityContext : DbContext, IDbContext
{
    private TableNameResult _result;

    public TableNameResult GetTableName()
    {
        if (_result != null)
        {
            return _result;
        }

        using var context = contextFactory.CreateDbContext();
        var entityType = context.Model.FindEntityType(typeof(TEntity)) ?? throw new Exception($"Entity type {typeof(TEntity).FullName} not found in the model.");

        var tableName = entityType.GetTableName() + "_Changes";
        var schema = entityType.GetSchema();
        _result = new TableNameResult(tableName, schema);

        return _result;
    }
}
