using System;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Implementations.JsonPatch;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Interfaces.Services.JsonPatch;

namespace Firebend.AutoCrud.Core.Abstractions.Builders
{
    public abstract class EntityCrudBuilder<TKey, TEntity> : EntityBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : IEntity<TKey>
    {
        public abstract Type CreateType { get; }

        public abstract Type ReadType { get; }

        public abstract Type SearchType { get; }

        public abstract Type UpdateType { get; }

        public abstract Type DeleteType { get; }

        public abstract Type SoftDeleteType { get; }
        
        public Type SearchRequestType { get; set; }

        protected abstract void ApplyPlatformTypes();

        public EntityCrudBuilder()
        {
            WithRegistration<IEntityDefaultOrderByProvider<TKey, TEntity>, DefaultEntityDefaultOrderByProvider<TKey, TEntity>>(false);
            WithRegistration<IEntityDomainEventPublisher, DefaultEntityDomainEventPublisher>(false);
            WithRegistration<IDomainEventContextProvider, DefaultDomainEventContextProvider>(false);
            WithRegistration<IJsonPatchDocumentGenerator, JsonPatchDocumentDocumentGenerator>(false);
        }

        protected override void OnBuild()
        {
            base.OnBuild();
            ApplyPlatformTypes();
        }
    }
}