using System.Data.Common;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IDbContextOptionsProvider<TKey, TEntity, TContext>
        where TKey : struct
        where TEntity : IEntity<TKey>
        where TContext : DbContext, IDbContext

    {
        DbContextOptions<TContext> GetDbContextOptions(string connectionString);

        DbContextOptions<TContext> GetDbContextOptions(DbConnection connection);

        DbContextOptions GetDbConnectionOptions(string connectionString); //todo find out how to get away from doing this when we have custom fields and change tracking

        DbContextOptions GetDbConnectionOptions(DbConnection connection);
    }
}
