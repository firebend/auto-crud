﻿using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Services;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Abstractions.Client;
using Firebend.AutoCrud.EntityFramework.ExceptionHandling;
using Firebend.AutoCrud.EntityFramework.Including;
using Firebend.AutoCrud.EntityFramework.Interfaces;

namespace Firebend.AutoCrud.CustomFields.EntityFramework
{
    public static class Extensions
    {
        public static EntityCrudBuilder<TKey, TEntity> AddCustomFields<TKey, TEntity>(
            this EntityFrameworkEntityBuilder<TKey, TEntity> builder)
            where TKey : struct
            where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>, new()
        {
            builder.WithRegistration<IDbContextProvider<Guid, CustomFieldsEntity<TKey, TEntity>>,
                AbstractCustomFieldsDbContextProvider<TKey, TEntity>>();

            builder.WithRegistration<IEntityFrameworkIncludesProvider<Guid, CustomFieldsEntity<TKey, TEntity>>,
                DefaultEntityFrameworkIncludesProvider<Guid, CustomFieldsEntity<TKey,TEntity>>>(false);

            builder.WithRegistration<IEntityFrameworkDbUpdateExceptionHandler<Guid, CustomFieldsEntity<TKey, TEntity>>,
                DefaultEntityFrameworkDbUpdateExceptionHandler<Guid, CustomFieldsEntity<TKey, TEntity>>>();

            builder.WithRegistration<ICustomFieldsCreateService<TKey, TEntity>,
                AbstractEfCustomFieldsCreateService<TKey, TEntity>>(false);

            builder.WithRegistration<IEntityFrameworkCreateClient<Guid, CustomFieldsEntity<TKey, TEntity>>,
                EntityFrameworkCreateClient<Guid, CustomFieldsEntity<TKey, TEntity>>>();

            builder.WithRegistration<ICustomFieldsDeleteService<TKey, TEntity>, AbstractCustomFieldsAlterService<TKey, TEntity>>(false);
            builder.WithRegistration<ICustomFieldsUpdateService<TKey, TEntity>, AbstractCustomFieldsAlterService<TKey, TEntity>>(false);

            builder.WithRegistration<ICustomFieldsSearchService<TKey, TEntity>, AbstractEfCustomFieldSearchService<TKey, TEntity>>(false);

            if (builder.IsTenantEntity)
            {
                var creatorType = typeof(AbstractTenantSqlServerCustomFieldsStorageCreator<,,>)
                    .MakeGenericType(builder.EntityKeyType, builder.EntityType, builder.TenantEntityKeyType);

                builder.WithRegistration<ICustomFieldsStorageCreator<TKey, TEntity>>(creatorType, false);
            }
            else
            {
                builder.WithRegistration<ICustomFieldsStorageCreator<TKey, TEntity>, AbstractSqlServerCustomFieldsStorageCreator<TKey, TEntity>>(false);
            }

            builder.WithRegistration<IEntityTableCreator, EntityTableCreator>(false);

            return builder;
        }
    }
}
