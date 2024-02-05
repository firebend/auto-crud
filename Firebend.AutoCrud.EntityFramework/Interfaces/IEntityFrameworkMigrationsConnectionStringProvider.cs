using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.EntityFramework.Interfaces;

/// <summary>
/// Provides an array of connection strings to run migrations against
/// </summary>
public interface IEntityFrameworkMigrationsConnectionStringProvider
{
    Task<string[]> GetConnectionStringsAsync(CancellationToken cancellationToken);
}
