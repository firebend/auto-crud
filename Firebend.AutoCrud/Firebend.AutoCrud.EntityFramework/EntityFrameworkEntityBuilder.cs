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
            CreateType = typeof(EntityFrameworkEntityCreateService<TKey,TEntity>);
            ReadType = typeof(EntityFrameworkEntityReadService<TKey, TEntity>);
            UpdateType = typeof(EntityFrameworkEntityUpdateService<TKey, TEntity>);
            
            DeleteType = IsActiveEntity ?
                typeof(EntityFrameworkEntitySoftDeleteService<,>).MakeGenericType(EntityKeyType, EntityType) :
                typeof(EntityFrameworkEntityDeleteService<TKey, TEntity>);
            
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
            if (IsTenantEntity)
            {
                WithRegistration<IEntityFrameworkCreateClient<TKey, TEntity>>(
                    typeof(EntityFrameworkTenantCreateClient<,,>).MakeGenericType(EntityKeyType, EntityType, TenantEntityKeyType), false);

                WithRegistration<IEntityFrameworkQueryClient<TKey, TEntity>>(
                    typeof(EntityFrameworkTenantQueryClient<,,>).MakeGenericType(EntityKeyType, EntityType, TenantEntityKeyType), false);

                WithRegistration<IEntityFrameworkUpdateClient<TKey, TEntity>>(
                    typeof(EntityFrameworkTenantUpdateClient<,,>).MakeGenericType(EntityKeyType, EntityType, TenantEntityKeyType), false);

                WithRegistration<IEntityFrameworkDeleteClient<TKey, TEntity>>(
                    typeof(EntityFrameworkTenantDeleteClient<,,>).MakeGenericType(EntityKeyType, EntityType, TenantEntityKeyType), false);
            }
            else
            {
                WithRegistration<IEntityFrameworkCreateClient<TKey, TEntity>, EntityFrameworkCreateClient<TKey, TEntity>>(false);
                WithRegistration<IEntityFrameworkQueryClient<TKey, TEntity>, EntityFrameworkQueryClient<TKey, TEntity>>(false);
                WithRegistration<IEntityFrameworkUpdateClient<TKey, TEntity>, EntityFrameworkUpdateClient<TKey, TEntity>>(false);
                WithRegistration<IEntityFrameworkDeleteClient<TKey, TEntity>, EntityFrameworkDeleteClient<TKey, TEntity>>(false);
            }

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