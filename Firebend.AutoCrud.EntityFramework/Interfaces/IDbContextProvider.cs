using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Interfaces
{
    // ReSharper disable once UnusedTypeParameter
    public interface IDbContextProvider<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        Task<IDbContext> GetDbContextAsync(CancellationToken cancellationToken = default);
    }
}
