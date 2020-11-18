using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Web.Sample.DbContexts
{
    public class PersonDbContext : DbContext, IDbContext
    {
        private static DbContextOptions GetOptions()
        {
            var connString = DataAccessConfiguration.GetConfiguration().GetConnectionString("InventoryElasticPool");

            return new DbContextOptionsBuilder()
                .UseSqlServer(connString)
                .Options;
        }
        public PersonDbContext() : base(GetOptions())
        {
        }

        public PersonDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<EfPerson> People { get; set; }

        public DbSet<EfPet> Pets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLoggerFactory(
                    LoggerFactory.Create(c => c.AddConsole()))
                .EnableSensitiveDataLogging();

            base.OnConfiguring(optionsBuilder);
        }
    }
}
