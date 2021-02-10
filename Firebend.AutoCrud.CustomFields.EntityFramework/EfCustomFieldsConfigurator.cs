using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.ExceptionHandling;
using Firebend.AutoCrud.EntityFramework.Including;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework
{
    public class EfCustomFieldsConfigurator<TBuilder, TKey, TEntity> : EntityCrudConfigurator<TBuilder, TKey, TEntity>
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
    {
        public EfCustomFieldsConfigurator(TBuilder builder) : base(builder)
        {
        }

        public EfCustomFieldsConfigurator<TBuilder, TKey, TEntity> AddCustomFields(Action<EntityFrameworkEntityBuilder<Guid, EfCustomFieldsModel<TKey, TEntity>>> configure = null)
        {
            AddCustomFields(Builder, false);

            if (configure != null)
            {
                var customFieldsBuilder = new EntityFrameworkEntityBuilder<Guid, EfCustomFieldsModel<TKey, TEntity>>();
                customFieldsBuilder.SignatureBase = $"{typeof(TEntity).Name}_CustomFields";
                configure(customFieldsBuilder);
                Builder.Registrations.Add(typeof(object), new List<Registration>
                {
                    new BuilderRegistration
                    {
                        Builder = customFieldsBuilder
                    }
                });
            }

            return this;
        }

        public EfCustomFieldsConfigurator<TBuilder, TKey, TEntity> AddCustomFieldsTenant<TTenantKey>(
            Action<EntityFrameworkEntityBuilder<Guid, EfCustomFieldsModelTenant<TKey, TEntity, TTenantKey>>> configure = null)
            where TTenantKey : struct
        {
            AddCustomFields(Builder, true);

            if (configure != null)
            {
                var customFieldsBuilder = new EntityFrameworkEntityBuilder<Guid, EfCustomFieldsModelTenant<TKey, TEntity, TTenantKey>>();
                customFieldsBuilder.SignatureBase = $"{typeof(TEntity).Name}_CustomFields";
                configure(customFieldsBuilder);
                Builder.Registrations.Add(typeof(object), new List<Registration>
                {
                    new BuilderRegistration
                    {
                        Builder = customFieldsBuilder
                    }
                });
            }

            return this;
        }

        private static TBuilder AddCustomFields(
            TBuilder builder,
            bool isTenantEntity)
        {
            var guidType = typeof(Guid);

            var efModelType = isTenantEntity
                ? typeof(EfCustomFieldsModelTenant<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, builder.TenantEntityKeyType)
                : typeof(EfCustomFieldsModel<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType);

            builder.WithRegistration(
                typeof(IDbContextProvider<,>).MakeGenericType(guidType, efModelType),
                typeof(AbstractCustomFieldsDbContextProvider<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
                false);

            builder.WithRegistration(
                typeof(IDbContextOptionsProvider<,>).MakeGenericType(guidType, efModelType),
                typeof(AbstractCustomFieldsDbContextOptionsProvider<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
                false);

            builder.WithRegistration(
                typeof(IDbContextConnectionStringProvider<,>).MakeGenericType(guidType, efModelType),
                typeof(AbstractCustomFieldsConnectionStringProvider<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
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
                isTenantEntity
                    ? typeof(EntityFrameworkTenantCreateClient<,,>).MakeGenericType(guidType, efModelType, builder.TenantEntityKeyType)
                    : typeof(EntityFrameworkCreateClient<,>).MakeGenericType(guidType, efModelType),
                false);

            builder.WithRegistration<ICustomFieldsUpdateService<TKey, TEntity>>(
                typeof(AbstractEfCustomFieldsUpdateService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
                false);

            builder.WithRegistration(
                typeof(IEntityFrameworkUpdateClient<,>).MakeGenericType(guidType, efModelType),
                isTenantEntity
                    ? typeof(EntityFrameworkTenantUpdateClient<,,>).MakeGenericType(guidType, efModelType, builder.TenantEntityKeyType)
                    : typeof(EntityFrameworkUpdateClient<,>).MakeGenericType(guidType, efModelType),
                false);

            builder.WithRegistration<ICustomFieldsDeleteService<TKey, TEntity>>(
                typeof(AbstractEfCustomFieldsDeleteService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
                false);

            builder.WithRegistration(
                typeof(IEntityFrameworkDeleteClient<,>).MakeGenericType(guidType, efModelType),
                isTenantEntity
                    ? typeof(EntityFrameworkTenantDeleteClient<,,>).MakeGenericType(guidType, efModelType, builder.TenantEntityKeyType)
                    : typeof(EntityFrameworkDeleteClient<,>).MakeGenericType(guidType, efModelType),
                false);

            builder.WithRegistration<ICustomFieldsSearchService<TKey, TEntity>>(
                typeof(AbstractEfCustomFieldSearchService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, efModelType),
                false);

            builder.WithRegistration(
                typeof(IEntityFrameworkQueryClient<,>).MakeGenericType(guidType, efModelType),
                isTenantEntity
                    ? typeof(EntityFrameworkTenantQueryClient<,,>).MakeGenericType(guidType, efModelType, builder.TenantEntityKeyType)
                    : typeof(EntityFrameworkQueryClient<,>).MakeGenericType(guidType, efModelType),
                false);

            builder.WithRegistration(
                typeof(IEntityQueryOrderByHandler<,>).MakeGenericType(guidType, efModelType),
                typeof(DefaultEntityQueryOrderByHandler<,>).MakeGenericType(guidType, efModelType),
                false);

            var creatorType = isTenantEntity
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
