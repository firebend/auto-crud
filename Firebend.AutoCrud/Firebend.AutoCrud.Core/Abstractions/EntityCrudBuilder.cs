using System;
namespace Firebend.AutoCrud.Core.Abstractions
{
    public abstract class EntityCrudBuilder : EntityBuilder
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