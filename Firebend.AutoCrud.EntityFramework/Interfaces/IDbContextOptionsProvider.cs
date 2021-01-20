using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IDbContextOptionsProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        DbContextOptions GetDbContextOptions(string connectionString);
    }
}
