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
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.JsonPatch;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Core.Abstractions.Services
{
    public abstract class AbstractCustomFieldsAlterService<TKey, TEntity> :
        BaseDisposable,
        //ICustomFieldsCreateService<TKey, TEntity>,
        ICustomFieldsDeleteService<TKey, TEntity>
        //ICustomFieldsUpdateService<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        private readonly IDomainEventContextProvider _domainEventContextProvider;
        private readonly IEntityDomainEventPublisher _eventPublisher;
        private readonly IJsonPatchGenerator _patchDocumentGenerator;
        private readonly IEntityReadService<TKey, TEntity> _readService;
        private readonly IEntityUpdateService<TKey, TEntity> _updateService;

        protected AbstractCustomFieldsAlterService(IEntityReadService<TKey, TEntity> readService,
            IEntityUpdateService<TKey, TEntity> updateService,
            IDomainEventContextProvider domainEventContextProvider,
            IEntityDomainEventPublisher eventPublisher,
            IJsonPatchGenerator patchDocumentGenerator)
        {
            _domainEventContextProvider = domainEventContextProvider;
            _eventPublisher = eventPublisher;
            _patchDocumentGenerator = patchDocumentGenerator;
            _updateService = updateService;
            _readService = readService;
        }

        private async Task<CustomFieldsEntity<TKey>> SaveAsync(
            TKey? entityKey,
            Func<TEntity, (TEntity, CustomFieldsEntity<TKey>)> updateFunc,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken)
        {
            if (!entityKey.HasValue || entityKey.Value.Equals(default(TKey)))
            {
                return null;
            }

            var rootEntity = await _readService.GetByKeyAsync(entityKey.Value, entityTransaction, cancellationToken);

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

            var entity = await _updateService.UpdateAsync(afterModified, cancellationToken).ConfigureAwait(false);

            await PublishDomainEventAsync(beforeModified, entity, entityTransaction, cancellationToken);

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

            fieldsEntity.CreatedDate = DateTimeOffset.UtcNow;
            fieldsEntity.ModifiedDate = DateTimeOffset.UtcNow;

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

            customFieldsEntity.ModifiedDate = DateTimeOffset.UtcNow;

            var old = rootEntity.CustomFields[index.Value];
            customFieldsEntity.CopyPropertiesTo(old, nameof(IModifiedEntity.CreatedDate));
            rootEntity.CustomFields[index.Value] = old;

            return (rootEntity, old);
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

            attribute.ModifiedDate = DateTimeOffset.UtcNow;

            rootEntity.CustomFields[index.Value] = attribute;

            return (rootEntity, attribute);
        }

        private Task PublishDomainEventAsync(TEntity beforeModified,
            TEntity entity,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken)
        {
            if (_eventPublisher == null || _eventPublisher is DefaultEntityDomainEventPublisher)
            {
                return Task.CompletedTask;
            }

            var patch = _patchDocumentGenerator?.Generate(beforeModified, entity);

            var domainEvent = new EntityUpdatedDomainEvent<TEntity>
            {
                Previous = beforeModified,
                OperationsJson = JsonConvert.SerializeObject(patch?.Operations, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }),
                EventContext = _domainEventContextProvider?.GetContext()
            };

            return _eventPublisher?.PublishEntityUpdatedEventAsync(domainEvent, entityTransaction, cancellationToken);

        }

        public Task<CustomFieldsEntity<TKey>> CreateAsync(TKey rootKey,
            CustomFieldsEntity<TKey> customField,
            CancellationToken cancellationToken = default)
            => CreateAsync(rootKey, customField, null, cancellationToken);

        public Task<CustomFieldsEntity<TKey>> CreateAsync(TKey rootEntityKey,
            CustomFieldsEntity<TKey> customField,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
        => SaveAsync(rootEntityKey, root => AddCustomAttribute(customField, root), entityTransaction, cancellationToken);

        public Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootKey,
            CustomFieldsEntity<TKey> customField,
            CancellationToken cancellationToken = default)
            => UpdateAsync(rootKey, customField, null, cancellationToken);

        public Task<CustomFieldsEntity<TKey>> UpdateAsync(TKey rootEntityKey,
            CustomFieldsEntity<TKey> customField,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
        => SaveAsync(rootEntityKey, root => UpdateCustomAttribute(customField.Id, customField, root), entityTransaction, cancellationToken);

        public Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootKey, Guid key, CancellationToken cancellationToken = default)
            => DeleteAsync(rootKey, key, null, cancellationToken);

        public Task<CustomFieldsEntity<TKey>> DeleteAsync(TKey rootEntityKey,
            Guid key,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
            => SaveAsync(rootEntityKey, root => RemoveCustomAttribute(key, root), entityTransaction, cancellationToken);

        public Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootKey,
            Guid key,
            JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
            CancellationToken cancellationToken = default)
            => PatchAsync(rootKey, key, jsonPatchDocument, null, cancellationToken);

        public Task<CustomFieldsEntity<TKey>> PatchAsync(TKey rootEntityKey,
            Guid key,
            JsonPatchDocument<CustomFieldsEntity<TKey>> jsonPatchDocument,
            IEntityTransaction entityTransaction,
            CancellationToken cancellationToken = default)
            => SaveAsync(rootEntityKey, root => UpdateCustomAttribute(key, jsonPatchDocument, root), entityTransaction, cancellationToken);


        protected override void DisposeManagedObjects()
        {
            _readService?.Dispose();
            _updateService?.Dispose();
        }
    }
}
