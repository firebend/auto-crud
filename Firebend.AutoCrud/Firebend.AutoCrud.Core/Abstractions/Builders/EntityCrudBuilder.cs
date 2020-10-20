using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

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

        protected override void OnBuild()
        {
            base.OnBuild();
            ApplyPlatformTypes();
        }
    }
}