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
    private static readonly Type _entityType = typeof(TEntity);
    private TableNameResult _result;

    public TableNameResult GetTableName()
    {
        if (_result != null)
        {
            return _result;
        }

        using var context = contextFactory.CreateDbContext();
        _result = GetTableName(context);

        return _result;
    }

    private TableNameResult GetTableName(DbContext context)
    {
        var entityType = context.Model.FindEntityType(_entityType) ??
                         throw new Exception($"Entity type {_entityType.FullName} not found in the model.");

        var tableName = entityType.GetTableName() + "_Changes";
        var schema = entityType.GetSchema() ?? context.Model.GetDefaultSchema() ?? "dbo";
        return new TableNameResult(tableName, schema);
    }
}
