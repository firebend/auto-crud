using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Interfaces;

/// <summary>
/// Provides an array of connection strings to run migrations against
/// </summary>
public interface IEntityFrameworkMigrationsConnectionStringProvider<TDbContext>
    where TDbContext : DbContext
{
    Task<string[]> GetConnectionStringsAsync(CancellationToken cancellationToken);
}
