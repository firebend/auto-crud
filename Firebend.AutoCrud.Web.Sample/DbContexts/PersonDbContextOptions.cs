using System.Data.Common;
using DocumentFormat.OpenXml.Bibliography;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.CustomCommands;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Web.Sample.DbContexts
{
    public static class PersonDbContextOptions
    {
        private static ILoggerFactory _loggerFactory;
        public static ILoggerFactory DbLoggerFactory => _loggerFactory ??= LoggerFactory.Create(c => c.AddConsole());

        public static DbContextOptions GetOptions() => GetOptions(DataAccessConfiguration.GetConfiguration().GetConnectionString("InventoryElasticPool"), DbLoggerFactory);

        public static DbContextOptions<PersonDbContext> GetOptions(string connectionString, ILoggerFactory loggerFactory) => new DbContextOptionsBuilder<PersonDbContext>()
            .UseSqlServer(connectionString)
            .AddFirebendFunctions()
            .UseLoggerFactory(loggerFactory)
            .EnableSensitiveDataLogging()
            .Options;

        public static DbContextOptions<PersonDbContext> GetOptions(DbConnection connection, ILoggerFactory loggerFactory) => new DbContextOptionsBuilder<PersonDbContext>()
            .UseSqlServer(connection)
            .AddFirebendFunctions()
            .UseLoggerFactory(loggerFactory)
            .EnableSensitiveDataLogging()
            .Options;
    }

    public class PersonDbContextOptionsProvider<TKey, TEntity> : IDbContextOptionsProvider<TKey, TEntity, PersonDbContext>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly ILoggerFactory _loggerFactory;

        public PersonDbContextOptionsProvider(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public DbContextOptions<PersonDbContext> GetDbContextOptions(string connectionString) => PersonDbContextOptions.GetOptions(connectionString, _loggerFactory);
        public DbContextOptions<PersonDbContext> GetDbContextOptions(DbConnection connection) => PersonDbContextOptions.GetOptions(connection, _loggerFactory);
    }
}
