using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Client
{
    public class EntityFrameworkUpdateClient<TKey, TEntity> : AbstractDbContextRepo<TKey, TEntity>, IEntityUpdateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        public EntityFrameworkUpdateClient(IDbContext context) : base(context)
        {
        }

        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var entry = Context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                var set = GetDbSet();

                var found = await set.FindAsync(entity.Id);

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

            await Context.SaveChangesAsync(cancellationToken);

            return entry.Entity;
        }

        public async Task<TEntity> PatchAsync(TKey key, JsonPatchDocument<TEntity> jsonPatchDocument, CancellationToken cancellationToken = default)
        {
            var entity = await GetByKeyAsync(key, cancellationToken);

            if (entity == null)
            {
                return null;
            }
            
            jsonPatchDocument.ApplyTo(entity);

            return await UpdateAsync(entity, cancellationToken);
        }
    }
}