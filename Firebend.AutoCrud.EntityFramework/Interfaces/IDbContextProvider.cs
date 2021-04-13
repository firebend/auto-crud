using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    // ReSharper disable once UnusedTypeParameter
    public interface IDbContextProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default);

        Task<IDbContext> GetDbContextAsync(DbConnection connection, CancellationToken cancellationToken = default);
    }
}
