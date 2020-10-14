using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.EntityFramework.Abstractions.Entities
{
    public abstract class EntityFrameworkEntitySoftDeleteService<TKey, TEntity> : IEntityDeleteService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, IActiveEntity, new()
    {
        private readonly IEntityUpdateService<TKey, TEntity> _updateService;

        public EntityFrameworkEntitySoftDeleteService(IEntityUpdateService<TKey, TEntity> updateService)
        {
            _updateService = updateService;
        }

        public Task<TEntity> DeleteAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var patch = new JsonPatchDocument<TEntity>();

            patch.Add(x => x.IsDeleted, true);

            return _updateService.PatchAsync(key, patch, cancellationToken);
        }
    }
}