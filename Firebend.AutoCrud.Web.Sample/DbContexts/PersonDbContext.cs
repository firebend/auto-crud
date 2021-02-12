using Firebend.AutoCrud.CustomFields.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.Web.Sample.DbContexts
{
    public class PersonDbContext : DbContext, IDbContext
    {

        public PersonDbContext() : base(PersonDbContextOptions.GetOptions())
        {
        }

        public PersonDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<EfPerson> People { get; set; }

        public DbSet<EfPet> Pets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            this.AddCustomFieldsConfigurations(modelBuilder);
        }
    }
}
