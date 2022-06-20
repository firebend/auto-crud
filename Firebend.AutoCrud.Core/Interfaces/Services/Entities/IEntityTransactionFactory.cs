using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Entities
{
    public interface IEntityTransactionFactory<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        Task<string> GetDbContextHashCode();
        Task<IEntityTransaction> StartTransactionAsync(CancellationToken cancellationToken);
        bool ValidateTransaction(IEntityTransaction transaction);
    }
}
