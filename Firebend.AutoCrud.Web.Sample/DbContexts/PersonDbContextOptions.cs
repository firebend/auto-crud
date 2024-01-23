using System.Data.Common;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.CustomCommands;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Web.Sample.DbContexts;

public static class PersonDbContextOptions
{
    private static ILoggerFactory _loggerFactory;
    public static ILoggerFactory DbLoggerFactory => _loggerFactory ??= LoggerFactory.Create(c => c.AddConsole());

    public static DbContextOptions<PersonDbContext> GetOptions() => GetOptions<PersonDbContext>(DataAccessConfiguration.GetConfiguration().GetConnectionString("Elastic"), DbLoggerFactory);

    public static DbContextOptions<TContext> GetOptions<TContext>(string connectionString, ILoggerFactory loggerFactory)
        where TContext : DbContext => new DbContextOptionsBuilder<TContext>()
        .UseSqlServer(connectionString)
        .AddFirebendFunctions()
        .UseLoggerFactory(loggerFactory)
        .EnableSensitiveDataLogging()
        .Options;

    public static DbContextOptions<TContext> GetOptions<TContext>(DbConnection connection, ILoggerFactory loggerFactory)
        where TContext : DbContext => new DbContextOptionsBuilder<TContext>()
        .UseSqlServer(connection)
        .AddFirebendFunctions()
        .UseLoggerFactory(loggerFactory)
        .EnableSensitiveDataLogging()
        .Options;
}

public class PersonDbContextOptionsProvider<TKey, TEntity> : IDbContextOptionsProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    private readonly ILoggerFactory _loggerFactory;

    public PersonDbContextOptionsProvider(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public DbContextOptions<TContext> GetDbContextOptions<TContext>(string connectionString)
        where TContext : DbContext => PersonDbContextOptions.GetOptions<TContext>(connectionString, _loggerFactory);
    public DbContextOptions<TContext> GetDbContextOptions<TContext>(DbConnection connection)
        where TContext : DbContext => PersonDbContextOptions.GetOptions<TContext>(connection, _loggerFactory);
}
