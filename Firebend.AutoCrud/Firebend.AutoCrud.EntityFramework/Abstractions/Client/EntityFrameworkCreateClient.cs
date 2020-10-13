using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public class EntityFrameworkCreateClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkCreateClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        public EntityFrameworkCreateClient(IDbContext context) : base(context)
        {
        }

        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            var entry = await GetDbSet()
                .AddAsync(entity, cancellationToken)
                .ConfigureAwait(false);
            
            var savedEntity = entry.Entity;

            await Context.SaveChangesAsync(cancellationToken);

            return savedEntity;
        }
    }
}