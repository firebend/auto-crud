using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Firebend.AutoCrud.Web.Sample.DbContexts
{
    public class PersonDbContext : DbContext, IDbContext
    {
        public PersonDbContext() : base(new DbContextOptionsBuilder().UseSqlServer("SqlServer").Options)
        {
        }

        public PersonDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<EfPerson> People { get; set; }

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
