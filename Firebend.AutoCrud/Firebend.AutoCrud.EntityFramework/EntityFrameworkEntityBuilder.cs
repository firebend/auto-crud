using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.EntityFramework.Abstractions.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework
{
    public class EntityFrameworkEntityBuilder : EntityCrudBuilder
    {
        public override Type CreateType { get; } = typeof(EntityFrameworkEntityCreateService<,>);
        
        public override Type ReadType { get; } = typeof(EntityFrameworkEntityReadService<,>);
        
        public override Type SearchType { get; } = typeof(EntityFrameworkEntitySearchService<,,>);
        
        public override Type UpdateType { get; } = typeof(EntityFrameworkEntityUpdateService<,>);
        
        public override Type DeleteType { get; } = typeof(EntityFrameworkEntityDeleteService<,>);
        
        public override Type SoftDeleteType { get; } = typeof(EntityFrameworkEntitySoftDeleteService<,>);
        
        protected override void ApplyPlatformTypes()
        {
            
        }

        public EntityFrameworkEntityBuilder WithDbContext(Type dbContextType)
        {
            return this.WithRegistration(typeof(IDbContext), dbContextType, typeof(IDbContext));
        }

        public EntityFrameworkEntityBuilder WithDbContext<TContext>()
            where TContext : IDbContext
        {
            return WithDbContext(typeof(TContext));
        }

    }
}