using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.Abstractions.Entities;
using Firebend.AutoCrud.EntityFramework.Indexing;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.EntityFramework
{
    public class EntityFrameworkEntityBuilder<TKey, TEntity> : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        public EntityFrameworkEntityBuilder()
        {
            CreateType = IsTenantEntity ? 
                typeof(EntityFrameworkTenantEntityCreateService<,,>).MakeGenericType(EntityKeyType, EntityType, TenantEntityKeyType) :
                typeof(EntityFrameworkEntityCreateService<TKey,TEntity>);

            ReadType = typeof(EntityFrameworkEntityReadService<,>);
            UpdateType = typeof(EntityFrameworkEntityUpdateService<,>);
            
            DeleteType = IsActiveEntity ?
                typeof(EntityFrameworkEntitySoftDeleteService<,>) :
                typeof(EntityFrameworkEntityDeleteService<,>);
            
            SearchType = typeof(EntityFrameworkEntitySearchService<,,>);
        }

        public override Type CreateType { get; }

        public override Type ReadType { get; }

        public override Type SearchType { get; }

        public override Type UpdateType { get; }

        public override Type DeleteType { get; }

        public Type DbContextType { get; set; }

        protected override void ApplyPlatformTypes()
        {
            WithRegistration<IEntityFrameworkCreateClient<TKey, TEntity>, EntityFrameworkCreateClient<TKey, TEntity>>(false);
            WithRegistration<IEntityFrameworkQueryClient<TKey, TEntity>, EntityFrameworkQueryClient<TKey, TEntity>>(false);
            WithRegistration<IEntityFrameworkUpdateClient<TKey, TEntity>, EntityFrameworkUpdateClient<TKey, TEntity>>(false);
            WithRegistration<IEntityFrameworkDeleteClient<TKey,TEntity>, EntityFrameworkDeleteClient<TKey,TEntity>>(false);
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

        public EntityFrameworkEntityBuilder<TKey, TEntity> WithSearchFilter(Expression<Func<TEntity, string, bool>> filter)
        {
            WithRegistrationInstance<IEntityFrameworkFullTextExpressionProvider<TKey, TEntity>>(
                new DefaultEntityFrameworkFullTextExpressionProvider<TKey, TEntity>(filter));

            return this;
        }
    }
}