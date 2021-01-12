using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    public interface IDbContextOptionsProvider<TKey, TEntity>
    {
        DbContextOptions GetDbContextOptions(string connectionString);
    }
}
