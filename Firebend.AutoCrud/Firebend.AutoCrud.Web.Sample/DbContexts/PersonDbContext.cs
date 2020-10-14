using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.Web.Sample.DbContexts
{
    public class PersonDbContext : DbContext, IDbContext
    {
        public DbSet<EfPerson> People { get; set; }

        public PersonDbContext(DbContextOptions options) : base(options)
        {
            
        }
    }
}