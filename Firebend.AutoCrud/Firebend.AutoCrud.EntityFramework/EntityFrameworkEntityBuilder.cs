using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Core.Models.ClassGeneration;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.Abstractions.Entities;
using Firebend.AutoCrud.EntityFramework.Indexing;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.EntityFramework
{
    public class EntityFrameworkEntityBuilder<TKey, TEntity> : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        public override Type CreateType { get; } = typeof(EntityFrameworkEntityCreateService<,>);

        public override Type ReadType { get; } = typeof(EntityFrameworkEntityReadService<,>);

        public override Type SearchType { get; } = typeof(EntityFrameworkEntitySearchService<,,>);

        public override Type UpdateType { get; } = typeof(EntityFrameworkEntityUpdateService<,>);

        public override Type DeleteType { get; } = typeof(EntityFrameworkEntityDeleteService<,>);

        public override Type SoftDeleteType { get; } = typeof(EntityFrameworkEntitySoftDeleteService<,>);

        public Type DbContextType { get; set; }

        protected override void ApplyPlatformTypes()
        {
            WithRegistration<IEntityFrameworkCreateClient<TKey, TEntity>, EntityFrameworkCreateClient<TKey, TEntity>>(false);
            WithRegistration<IEntityFrameworkQueryClient<TKey, TEntity>, EntityFrameworkQueryClient<TKey, TEntity>>(false);
            WithRegistration<IEntityFrameworkUpdateClient<TKey, TEntity>, EntityFrameworkUpdateClient<TKey, TEntity>>(false);
            WithRegistration<IEntityFrameworkDeleteClient<TKey,TEntity>, EntityFrameworkDeleteClient<TKey,TEntity>>(false);
            WithRegistration<IEntityDefaultOrderByProvider<TKey, TEntity>, DefaultEntityDefaultOrderByProvider<TKey, TEntity>>(false);
            WithRegistration<IEntityDomainEventPublisher, DefaultEntityDomainEventPublisher>(false);
            WithRegistration<IEntityFrameworkFullTextExpressionProvider<TKey, TEntity>,DefaultEntityFrameworkFullTextExpressionProvider<TKey, TEntity>>(false);
        }

        public EntityFrameworkEntityBuilder<TKey, TEntity> WithDbContext(Type dbContextType)
        {
            DbContextType = dbContextType;

            var t = typeof(DbContextProvider<,,>).MakeGenericType(EntityKeyType, EntityType, dbContextType);

            WithRegistration<IDbContextProvider<TKey, TEntity>>(t);

            return this;
        }

        public EntityFrameworkEntityBuilder<TKey, TEntity> WithDbContext<TContext>()
            where TContext : IDbContext
        {
            return WithDbContext(typeof(TContext));
        }
        
        
        public EntityFrameworkEntityBuilder<TKey, TEntity> WithSearchFilter(Type type)
        {
            WithRegistration<IEntityFrameworkFullTextExpressionProvider<TKey, TEntity>>(type);
            return this;
        }

        public EntityFrameworkEntityBuilder<TKey, TEntity> WithSearchFilter<T>()
        {
            return WithSearchFilter(typeof(T));
        }

        public EntityFrameworkEntityBuilder<TKey, TEntity> WithSearchFilter(Expression<Func<string, TEntity, bool>> filter)
        {
            var signature = $"{SignatureBase}_SearchFilter";

            var iFaceType = typeof(IEntityFrameworkFullTextExpressionProvider<TKey,TEntity>);
            
            var propertySet = new PropertySet<Expression<Func<string, TEntity, bool>>>
            {
                Name = nameof(IEntityFrameworkFullTextExpressionProvider<Guid, FooEntity>.Filter),
                Value = filter,
                Override = true
            };

            WithDynamicClass(iFaceType, new DynamicClassRegistration
            {
                Interface = iFaceType,
                Properties = new [] { propertySet },
                Signature = signature,
                Lifetime = ServiceLifetime.Singleton
            });

            return this;
        }
    }
}