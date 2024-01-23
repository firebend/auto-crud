using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.EntityFramework.Interfaces;

public interface IDbContextConnectionStringProvider<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    Task<string> GetConnectionStringAsync(CancellationToken cancellationToken = default);
}
