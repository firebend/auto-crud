using Firebend.AutoCrud.EntityFramework.Abstractions;
using Firebend.AutoCrud.EntityFramework.Sample.Models;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Sample.DbContexts;

public sealed class AppDbContext : AbstractDbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
        People = Set<Person>();
        Pets = Set<Pet>();
    }

    public DbSet<Person> People { get; }

    public DbSet<Pet> Pets { get; }
}
