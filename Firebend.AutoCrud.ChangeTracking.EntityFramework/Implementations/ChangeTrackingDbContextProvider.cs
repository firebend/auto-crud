using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.DbContexts;
using Firebend.AutoCrud.ChangeTracking.EntityFramework.Interfaces;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Client;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Implementations;

public class ChangeTrackingDbContextProvider<TEntityKey, TEntity> :
    DbContextProvider<Guid, ChangeTrackingEntity<TEntityKey, TEntity>, ChangeTrackingDbContext<TEntityKey, TEntity>>,
    IChangeTrackingDbContextProvider<TEntityKey, TEntity>
    where TEntity : class, IEntity<TEntityKey>
    where TEntityKey : struct
{
    private readonly IDbContextConnectionStringProvider<TEntityKey, TEntity> _rootConnectionStringProvider;

    public ChangeTrackingDbContextProvider(
        IDbContextFactory<ChangeTrackingDbContext<TEntityKey, TEntity>> contextFactory,
        IDbContextConnectionStringProvider<TEntityKey, TEntity> connectionStringProvider = null) :
        base(contextFactory)
    {
        _rootConnectionStringProvider = connectionStringProvider;
    }

    protected override void InitDb(DbContext dbContext)
    {
        base.InitDb(dbContext);
        ScaffoldDbContext(dbContext);
    }

    protected override async Task<string> ProvideConnectionString(CancellationToken cancellationToken)
    {
        if (_rootConnectionStringProvider is null)
        {
            return null;
        }

        return await _rootConnectionStringProvider.GetConnectionStringAsync(cancellationToken);
    }

    private static void ScaffoldDbContext(DbContext context)
        => ChangeTrackingDbContextProviderCache.ScaffoldCache.GetOrAdd(typeof(TEntity).FullName,
            ScaffoldCacheFactory,
            context);

    private static bool ScaffoldCacheFactory(string typeName, DbContext dbContext)
    {
        var type = dbContext.Model.FindEntityType(typeof(ChangeTrackingEntity<TEntityKey, TEntity>))
                   ?? throw new Exception("Could not find entity type.");

        var schema = type.GetSchema().Coalesce("dbo");
        var table = type.GetTableName();

        if (dbContext.Database.GetService<IDatabaseCreator>() is not RelationalDatabaseCreator dbCreator)
        {
            return true;
        }

        var exists = DoesTableExist(dbContext, schema, table);

        if (!exists)
        {
            dbCreator.CreateTables();
        }

        return true;
    }

    private static bool DoesTableExist(DbContext context, string schemaName, string tableName)
    {
        var conn = context.Database.GetDbConnection();

        if (conn.State == ConnectionState.Closed)
        {
            conn.Open();
        }

        using var command = conn.CreateCommand();

        command.CommandText = $"""

                                   SELECT 1 FROM sys.tables AS T
                                       INNER JOIN sys.schemas AS S ON T.schema_id = S.schema_id
                                   WHERE S.Name = '{schemaName}' AND T.Name = '{tableName}'
                               """;

        var exists = command.ExecuteScalar() != null;

        return exists;
    }
}
