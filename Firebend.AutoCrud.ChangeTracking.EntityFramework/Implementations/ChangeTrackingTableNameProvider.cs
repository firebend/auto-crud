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
    private string _tableName;
    private string _schema;

    public (string Table, string Schema) GetTableName()
    {
        if (!string.IsNullOrWhiteSpace(_tableName))
        {
            return (_tableName, _schema);
        }

        using var context = contextFactory.CreateDbContext();
        var entityType = context.Model.FindEntityType(typeof(TEntity)) ?? throw new Exception($"Entity type {typeof(TEntity).FullName} not found in the model.");

        _tableName = entityType.GetTableName() + "_Changes";
        _schema = entityType.GetSchema();

        return (_tableName, _schema);
    }
}
