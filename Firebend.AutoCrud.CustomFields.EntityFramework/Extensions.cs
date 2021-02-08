using System;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.ExceptionHandling;
using Firebend.AutoCrud.EntityFramework.Including;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework
{
    public static class Extensions
    {
        public static EntityFrameworkEntityBuilder<TKey, TEntity> AddCustomFields<TKey, TEntity>(
            this EntityFrameworkEntityBuilder<TKey, TEntity> builder)
            where TKey : struct
            where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>, new()
        {
            var guidType = typeof(Guid);

            var efModelType = builder.IsTenantEntity
                ? typeof(EfCustomFieldsModelTenant<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, builder.TenantEntityKeyType)
                : typeof(EfCustomFieldsModel<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType);

            builder.WithRegistration(
                typeof(IDbContextProvider<,>).MakeGenericType(guidType, efModelType),
                typeof(AbstractCustomFieldsDbContextProvider<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
                false);

            builder.WithRegistration(
                typeof(IEntityFrameworkIncludesProvider<,>).MakeGenericType(guidType, efModelType),
                typeof(DefaultEntityFrameworkIncludesProvider<,>).MakeGenericType(guidType, efModelType),
                false);

            builder.WithRegistration(
                typeof(IEntityFrameworkDbUpdateExceptionHandler<,>).MakeGenericType(guidType, efModelType),
                typeof(DefaultEntityFrameworkDbUpdateExceptionHandler<,>).MakeGenericType(guidType, efModelType),
                false);

            builder.WithRegistration<ICustomFieldsCreateService<TKey, TEntity>>(
                typeof(AbstractEfCustomFieldsCreateService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
                false);

            builder.WithRegistration(
                typeof(IEntityFrameworkCreateClient<,>).MakeGenericType(guidType, efModelType),
                builder.IsTenantEntity
                    ? typeof(EntityFrameworkTenantCreateClient<,,>).MakeGenericType(guidType, efModelType, builder.TenantEntityKeyType)
                    : typeof(EntityFrameworkCreateClient<,>).MakeGenericType(guidType, efModelType),
                false);

            builder.WithRegistration<ICustomFieldsUpdateService<TKey, TEntity>>(
                typeof(AbstractEfCustomFieldsUpdateService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
                false);

            builder.WithRegistration(
                typeof(IEntityFrameworkUpdateClient<,>).MakeGenericType(guidType, efModelType),
                builder.IsTenantEntity
                    ? typeof(EntityFrameworkTenantUpdateClient<,,>).MakeGenericType(guidType, efModelType, builder.TenantEntityKeyType)
                    : typeof(EntityFrameworkUpdateClient<,>).MakeGenericType(guidType, efModelType),
                false);

            builder.WithRegistration<ICustomFieldsDeleteService<TKey, TEntity>>(
                typeof(AbstractEfCustomFieldsDeleteService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
                false);

            builder.WithRegistration(
                typeof(IEntityFrameworkDeleteClient<,>).MakeGenericType(guidType, efModelType),
                builder.IsTenantEntity
                    ? typeof(EntityFrameworkTenantDeleteClient<,,>).MakeGenericType(guidType, efModelType, builder.TenantEntityKeyType)
                    : typeof(EntityFrameworkDeleteClient<,>).MakeGenericType(guidType, efModelType),
                false);

            builder.WithRegistration<ICustomFieldsSearchService<TKey, TEntity>>(
                typeof(AbstractEfCustomFieldSearchService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
                false);

            builder.WithRegistration(
                typeof(IEntityFrameworkQueryClient<,>).MakeGenericType(guidType, efModelType),
                builder.IsTenantEntity
                    ? typeof(EntityFrameworkTenantQueryClient<,,>).MakeGenericType(guidType, efModelType, builder.TenantEntityKeyType)
                    : typeof(EntityFrameworkQueryClient<,>).MakeGenericType(guidType, efModelType),
                false);

            builder.WithRegistration(
                typeof(IEntityQueryOrderByHandler<,>).MakeGenericType(guidType, efModelType),
                typeof(DefaultEntityQueryOrderByHandler<,>).MakeGenericType(guidType, efModelType),
                false);

            var creatorType = builder.IsTenantEntity
                ? typeof(AbstractTenantSqlServerCustomFieldsStorageCreator<,,,>)
                    .MakeGenericType(builder.EntityKeyType, builder.EntityType, builder.TenantEntityKeyType, efModelType)
                : typeof(AbstractSqlServerCustomFieldsStorageCreator<,,>)
                    .MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType);

            builder.WithRegistration<ICustomFieldsStorageCreator<TKey, TEntity>>(creatorType, false);


            builder.WithRegistration<IEntityTableCreator, EntityTableCreator>(false);

            return builder;
        }
    }
}
