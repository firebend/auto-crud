using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.Abstractions.Entities;
using Firebend.AutoCrud.EntityFramework.Indexing;
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
            this.WithRegistration(typeof(IEntityFrameworkCreateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(EntityFrameworkCreateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IEntityFrameworkCreateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                false);

            this.WithRegistration(typeof(IEntityFrameworkQueryClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(EntityFrameworkQueryClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IEntityFrameworkQueryClient<,>).MakeGenericType(EntityKeyType, EntityType),
                false);

            this.WithRegistration(typeof(IEntityFrameworkUpdateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(EntityFrameworkUpdateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IEntityFrameworkUpdateClient<,>).MakeGenericType(EntityKeyType, EntityType),
                false);

            this.WithRegistration(typeof(IEntityFrameworkDeleteClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(EntityFrameworkDeleteClient<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IEntityFrameworkDeleteClient<,>).MakeGenericType(EntityKeyType, EntityType),
                false);

            this.WithRegistration(typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(DefaultEntityDefaultOrderByProvider<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(EntityKeyType, EntityType),
                false);

            this.WithRegistration<EntityFrameworkEntityBuilder,
                IEntityFrameworkFullTextExpressionProvider,
                DefaultEntityFrameworkFullTextExpressionProvider>(false);
        }

        public EntityFrameworkEntityBuilder WithDbContext(Type dbContextType)
        {
            return this.WithRegistration(typeof(IDbContextProvider<,>).MakeGenericType(EntityKeyType, EntityType),
                typeof(DbContextProvider<,,>).MakeGenericType(EntityKeyType, EntityType, dbContextType),
                typeof(IDbContextProvider<,>).MakeGenericType(EntityKeyType, EntityType));
        }

        public EntityFrameworkEntityBuilder WithDbContext<TContext>()
            where TContext : IDbContext
        {
            return WithDbContext(typeof(TContext));
        }
    }
}