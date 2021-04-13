using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Implmentations
{
    public class EntityFrameworkEntityTransactionFactory<TKey, TEntity> : IEntityTransactionFactory<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        private readonly IDbContextProvider<TKey, TEntity> _dbContextProvider;

        public EntityFrameworkEntityTransactionFactory(IDbContextProvider<TKey, TEntity> dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<IEntityTransaction> StartTransactionAsync(CancellationToken cancellationToken)
        {
            var context = await _dbContextProvider.GetDbContextAsync(cancellationToken);
            var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            return new EntityFrameworkEntityTransaction(transaction);
        }
    }
}
