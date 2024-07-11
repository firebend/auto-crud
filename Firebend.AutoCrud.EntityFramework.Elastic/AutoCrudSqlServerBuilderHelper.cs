using System;
using Firebend.AutoCrud.EntityFramework.CustomCommands;
using Firebend.AutoCrud.EntityFramework.Elastic.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Firebend.AutoCrud.EntityFramework.Elastic;

public class SqlServerBuilder
{
    public static Action<IServiceProvider, DbContextOptionsBuilder> Get(
        Action<DbContextOptionsBuilder> configureBuilder = null,
        Action<SqlServerDbContextOptionsBuilder> configureSqlServer = null) =>
        (provider, builder) =>
        {
            builder.UseSqlServer(o =>
                {
                    o.ExecutionStrategy(dependencies => new AutoCrudAzureExecutionStrategy(provider,
                        dependencies,
                        6,
                        TimeSpan.FromSeconds(30)));

                    configureSqlServer?.Invoke(o);
                })
                .AddFirebendFunctions();

            configureBuilder?.Invoke(builder);
        };
}
