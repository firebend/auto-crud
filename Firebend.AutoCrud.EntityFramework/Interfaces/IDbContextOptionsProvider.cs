using System.Data.Common;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IDbContextOptionsProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>

    {
        DbContextOptions<TContext> GetDbContextOptions<TContext>(string connectionString)
            where TContext : DbContext;

        DbContextOptions<TContext> GetDbContextOptions<TContext>(DbConnection connection)
            where TContext : DbContext;
    }
}
