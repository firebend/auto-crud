using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.EntityFramework.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Sample.DbContexts;

public sealed class AppDbContext : DbContext, IDbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
        People = Set<Person>();
        Pets = Set<Pet>();
    }

    public DbSet<Person> People { get; }

    public DbSet<Pet> Pets { get; }
}
