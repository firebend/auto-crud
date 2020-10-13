using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public class EntityFrameworkDeleteClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityFrameworkDeleteClient<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        public EntityFrameworkDeleteClient(IDbContextProvider<TKey, TEntity> contextProvider) : base(contextProvider)
        {
        }

        public async Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken)
        {
            var entity = new TEntity
            {
                Id = key
            };

            var entry = Context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                var set = GetDbSet();

                var found = await GetByKeyAsync(key, cancellationToken);

                if (found != null)
                {
                    entity = found;
                    set.Remove(found);
                }
                else
                {
                    set.Attach(entity);
                    entry.State = EntityState.Deleted;
                    entity = entry.Entity;
                }

            }
            else
            {
                entity = entry.Entity;
                entry.State = EntityState.Deleted;
            }

            await Context.SaveChangesAsync(cancellationToken);

            return entity;
        }
    }
}