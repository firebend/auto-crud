using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Abstractions
{
    public class AbstractDbContextRepo<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {

        private readonly IDbContext _context;
        
        public AbstractDbContextRepo(IDbContext context)
        {
            _context = context;
        }

        private DbSet<TEntity> GetDbSet()
        {
            return _context.Set<TEntity>();
        }

        public Task<TEntity> GetByKeyAsync(TKey key, CancellationToken cancellationToken)
        {
            return GetDbSet().FindAsync(key).AsTask();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await GetDbSet().ToArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            var e = (await GetDbSet().AddAsync(entity, cancellationToken).ConfigureAwait(false)).Entity;

            await _context.SaveChangesAsync(cancellationToken);

            return e;
        }

        public async Task<TEntity> UpdateAsync(TKey key, TEntity entity, CancellationToken cancellationToken)
        {
            entity.Id = key;

            var entry = _context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                var set = GetDbSet();

                var found = await set.FindAsync(key);

                if (found != null)
                {
                    entity.CopyPropertiesTo(found, "Id");
                }
                else
                {
                    set.Attach(entity);
                    entry.State = EntityState.Modified;
                }

            }

            await _context.SaveChangesAsync(cancellationToken);

            return entry.Entity;
        }

        public async Task DeleteAsync(TKey key, CancellationToken cancellationToken)
        {
            var entity = new TEntity
            {
                Id = key
            };

            var entry = _context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                var set = GetDbSet();

                var found = await set.FindAsync(key);

                if (found != null)
                {
                    set.Remove(found);
                }
                else
                {
                    set.Attach(entity);
                    entry.State = EntityState.Deleted;
                }

            }

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}