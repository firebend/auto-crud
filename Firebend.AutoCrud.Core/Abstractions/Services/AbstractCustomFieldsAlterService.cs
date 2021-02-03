using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Interfaces.Services.JsonPatch;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Core.Abstractions.Services
{
    public abstract class AbstractCustomFieldsAlterService<TKey, TEntity> :
        BaseDisposable,
        ICustomFieldsCreateService<TKey, TEntity>,
        ICustomFieldsDeleteService<TKey, TEntity>,
        ICustomFieldsUpdateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        private readonly IDomainEventContextProvider _domainEventContextProvider;
        private readonly IEntityDomainEventPublisher _eventPublisher;
        private readonly IJsonPatchDocumentGenerator _patchDocumentGenerator;
        private readonly IEntityReadService<TKey, TEntity> _readService;
        private readonly IEntityUpdateService<TKey, TEntity> _updateService;
        private readonly ICustomFieldsStorageCreator<TKey, TEntity> _customFieldsStorageCreator;

        protected AbstractCustomFieldsAlterService(IEntityReadService<TKey, TEntity> readService,
            IEntityUpdateService<TKey, TEntity> updateService,
            IDomainEventContextProvider domainEventContextProvider,
            IEntityDomainEventPublisher eventPublisher,
            IJsonPatchDocumentGenerator patchDocumentGenerator,
            ICustomFieldsStorageCreator<TKey, TEntity> customFieldsStorageCreator = null)
        {
            _domainEventContextProvider = domainEventContextProvider;
            _eventPublisher = eventPublisher;
            _patchDocumentGenerator = patchDocumentGenerator;
            _customFieldsStorageCreator = customFieldsStorageCreator;
            _updateService = updateService;
            _readService = readService;
        }

        private async Task<CustomFieldsEntity<TKey>> SaveAsync(
            TKey? entityKey,
            Func<TEntity, (TEntity, CustomFieldsEntity<TKey>)> updateFunc,
            CancellationToken cancellationToken)
        {
            if (!entityKey.HasValue || entityKey.Value.Equals(default(TKey)))
            {
                return null;
            }

            var rootEntity = await _readService.GetByKeyAsync(entityKey.Value, cancellationToken);

            if (rootEntity == null)
            {
                return null;
            }

            var beforeModified = rootEntity.Clone();
            var (afterModified, customAttributeEntity) = updateFunc(rootEntity);

            if (afterModified == null || customAttributeEntity == null)
            {
                return null;
            }

            if (_customFieldsStorageCreator != null)
            {
                await _customFieldsStorageCreator.CreateIfNotExistsAsync(cancellationToken).ConfigureAwait(false);
            }

            var entity = await _updateService.UpdateAsync(afterModified, cancellationToken).ConfigureAwait(false);

            await PublishDomainEventAsync(beforeModified, cancellationToken, entity);

            return customAttributeEntity;
        }

        private static (TEntity, CustomFieldsEntity<TKey>) AddCustomAttribute(CustomFieldsEntity<TKey> fieldsEntity, TEntity rootEntity)
        {
            if (rootEntity == null)
            {
                return (null, null);
            }

            rootEntity.CustomFields ??= new List<CustomFieldsEntity<TKey>>();

            if (fieldsEntity.Id == default)
            {
                fieldsEntity.Id = Guid.NewGuid();
            }

            rootEntity.CustomFields.Add(fieldsEntity);

            return (rootEntity, fieldsEntity);
        }

        private static (TEntity, CustomFieldsEntity<TKey>) RemoveCustomAttribute(Guid customAttributeId, TEntity rootEntity)
        {
            var index = GetIndex(customAttributeId, rootEntity);

            if (index == null)
            {
                return (null, null);
            }

            var attribute = rootEntity.CustomFields[index.Value];
            rootEntity.CustomFields.RemoveAt(index.Value);

            return (rootEntity, attribute);
        }

        private static int? GetIndex(Guid customAttributeId, TEntity rootEntity)
        {
            if (rootEntity?.CustomFields == null)
            {
                return null;
            }

            var index = rootEntity.CustomFields.FindIndex(x => x.Id == customAttributeId);

            if (index < 0)
            {
                return null;
            }

            return index;
        }

        private static (TEntity, CustomFieldsEntity<TKey>) UpdateCustomAttribute(Guid customAttributeId,
            CustomFieldsEntity<TKey> customFieldsEntity,
            TEntity rootEntity)
        {
            var index = GetIndex(customAttributeId, rootEntity);

            if (index == null)
            {
                return (null, null);
            }

            rootEntity.CustomFields[index.Value] = customFieldsEntity;

            return (rootEntity, customFieldsEntity);
        }

        private static (TEntity, CustomFieldsEntity<TKey>) UpdateCustomAttribute(Guid customAttributeId,
            JsonPatchDocument<CustomFieldsEntity<TKey>> patchDocument,
            TEntity rootEntity)
        {
            var index = GetIndex(customAttributeId, rootEntity);

            if (index == null)
            {
                return (null, null);
            }

            var attribute = rootEntity.CustomFields[index.Value];
            patchDocument.ApplyTo(attribute);

            rootEntity.CustomFields[index.Value] = attribute;

            return (rootEntity, attribute);
        }

        private Task PublishDomainEventAsync(TEntity beforeModified, CancellationToken cancellationToken, TEntity entity)
        {
            if (_eventPublisher == null || _eventPublisher is DefaultEntityDomainEventPublisher)
            {
                return Task.CompletedTask;
            }

            var domainEvent = new EntityUpdatedDomainEvent<TEntity>
            {
                Previous = entity,
                Patch = _patchDocumentGenerator?.Generate(beforeModified, entity),
                EventContext = _domainEventContextProvider?.GetContext()
            };

            return _eventPublisher?.PublishEntityUpdatedEventAsync(domainEvent, cancellationToken);

        }

        public Task<CustomFieldsEntity<TKey>> CreateAsync(TKey rootKey, CustomFieldsEntity<TKey> entity, CancellationToken cancellationToken = default)
            => SaveAsync(rootKey, root => AddCustomAttribute(entity, root), cancellationToken);

        public Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootKey,  CustomFieldsEntity<TKey> entity, CancellationToken cancellationToken = default)
            => SaveAsync(rootKey, root => UpdateCustomAttribute(entity.Id, entity, root),  cancellationToken);

        public Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootKey, Guid key, CancellationToken cancellationToken = default)
            => SaveAsync(rootKey, root => RemoveCustomAttribute(key, root), cancellationToken);

        public Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootKey,
            Guid key,
            JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
            CancellationToken cancellationToken = default)
            => SaveAsync(rootKey, root => UpdateCustomAttribute(key, jsonPatchDocument, root), cancellationToken);


        protected override void DisposeManagedObjects()
        {
            _readService?.Dispose();
            _updateService?.Dispose();
        }
    }
}
