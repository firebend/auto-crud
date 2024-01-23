using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.EntityFramework.Interfaces;

public interface IEntityTableCreator
{
    Task<bool> EnsureExistsAsync<TEntity>(IDbContext dbContext, CancellationToken cancellationToken);
}
