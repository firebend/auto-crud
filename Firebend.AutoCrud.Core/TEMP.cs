using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core
{
    public interface ICustomAttributeEntity<TKey>
    {
        List<CustomAttributeEntity<TKey>> CustomAttributes { get; set; }
    }

    public class CustomAttributeEntity<TKey> : IEntity<Guid>
    {
        public Guid Id { get; set; }

        public TKey EntityId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }

    public interface ICustomAttributeStorageCreator<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        Task CreateIfNotExistsAsync(CancellationToken cancellationToken);
    }

    public abstract class CustomAttributeCreateService<TKey, TEntity> : IEntityCreateService<Guid, CustomAttributeEntity<TKey>>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ICustomAttributeEntity<TKey>
    {
        private readonly IEntityReadService<TKey, TEntity> _entityReadService;

        protected CustomAttributeCreateService(IEntityReadService<TKey, TEntity> entityReadService)
        {
            _entityReadService = entityReadService;
        }

        protected abstract Task<CustomAttributeEntity<TKey>> SaveAsync(TEntity entity,
            CustomAttributeEntity<TKey> customAttributeEntity,
            CancellationToken cancellationToken);

        public async Task<CustomAttributeEntity<TKey>> CreateAsync(CustomAttributeEntity<TKey> entity, CancellationToken cancellationToken = default)
        {
            if (entity.EntityId.Equals(default(TKey)))
            {
                return null;
            }

            var rootEntity = await _entityReadService
                .GetByKeyAsync(entity.EntityId, cancellationToken)
                .ConfigureAwait(false);

            if (rootEntity == null)
            {
                return null;
            }

            if (rootEntity.CustomAttributes == null)
            {
                rootEntity.CustomAttributes = new List<CustomAttributeEntity<TKey>>();
            }

            rootEntity.CustomAttributes.Add(entity);

            var saved = await SaveAsync(rootEntity, entity, cancellationToken).ConfigureAwait(false);
            return saved;
        }

        public void Dispose() => _entityReadService?.Dispose();
    }
}
