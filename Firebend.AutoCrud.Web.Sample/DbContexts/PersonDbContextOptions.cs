using Firebend.AutoCrud.EntityFramework.Elastic.CustomCommands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Web.Sample.DbContexts
{
    public static class PersonDbContextOptions
    {
        public static DbContextOptions GetOptions() => GetOptions(DataAccessConfiguration.GetConfiguration().GetConnectionString("InventoryElasticPool"));

        public static DbContextOptions GetOptions(string connectionString) => new DbContextOptionsBuilder()
            .UseSqlServer(connectionString)
            .AddFirebendFunctions()
            .UseLoggerFactory(
                LoggerFactory.Create(c => c.AddConsole()))
            .EnableSensitiveDataLogging()
            .Options;
    }
}
