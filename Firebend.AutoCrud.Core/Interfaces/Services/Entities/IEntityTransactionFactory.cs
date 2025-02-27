using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities;

public interface IEntityTransactionFactory<TKey, TEntity>
    where TKey : struct
    where TEntity : IEntity<TKey>
{
    public Task<string> GetDbContextHashCode(CancellationToken cancellationToken);
    public Task<IEntityTransaction> StartTransactionAsync(CancellationToken cancellationToken);
    public bool ValidateTransaction(IEntityTransaction transaction);
}
