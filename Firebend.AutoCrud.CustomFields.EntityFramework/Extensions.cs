using System;
using Firebend.AutoCrud.Core.Implementations.Defaults;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
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
        public static EntityFrameworkEntityBuilder<TKey, TEntity> AddCustomFields<TKey, TEntity>(
            this EntityFrameworkEntityBuilder<TKey, TEntity> builder)
            where TKey : struct
            where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>, new()
        {
            builder.WithRegistration<IDbContextProvider<Guid, CustomFieldsEntity<TKey, TEntity>>,
                AbstractCustomFieldsDbContextProvider<TKey, TEntity>>();

            builder.WithRegistration<IEntityFrameworkIncludesProvider<Guid, CustomFieldsEntity<TKey, TEntity>>,
                DefaultEntityFrameworkIncludesProvider<Guid, CustomFieldsEntity<TKey, TEntity>>>(false);

            builder.WithRegistration<IEntityFrameworkDbUpdateExceptionHandler<Guid, CustomFieldsEntity<TKey, TEntity>>,
                DefaultEntityFrameworkDbUpdateExceptionHandler<Guid, CustomFieldsEntity<TKey, TEntity>>>(false);

            builder.WithRegistration<ICustomFieldsCreateService<TKey, TEntity>,
                AbstractEfCustomFieldsCreateService<TKey, TEntity>>(false);

            builder.WithRegistration<IEntityFrameworkCreateClient<Guid, CustomFieldsEntity<TKey, TEntity>>,
                EntityFrameworkCreateClient<Guid, CustomFieldsEntity<TKey, TEntity>>>(false);

            builder.WithRegistration<IEntityFrameworkUpdateClient<Guid, CustomFieldsEntity<TKey, TEntity>>,
                EntityFrameworkUpdateClient<Guid, CustomFieldsEntity<TKey, TEntity>>>(false);

            builder.WithRegistration<ICustomFieldsUpdateService<TKey, TEntity>,
                AbstractEfCustomFieldsUpdateService<TKey, TEntity>>(false);

            builder.WithRegistration<IEntityFrameworkDeleteClient<Guid, CustomFieldsEntity<TKey, TEntity>>,
                EntityFrameworkDeleteClient<Guid, CustomFieldsEntity<TKey, TEntity>>>(false);

            builder.WithRegistration<ICustomFieldsDeleteService<TKey, TEntity>,
                AbstractEfCustomFieldsDeleteService<TKey, TEntity>>(false);

            builder.WithRegistration<IEntityFrameworkQueryClient<Guid, CustomFieldsEntity<TKey, TEntity>>,
                EntityFrameworkQueryClient<Guid, CustomFieldsEntity<TKey, TEntity>>>();

            builder.WithRegistration<IEntityFrameworkIncludesProvider<Guid, CustomFieldsEntity<TKey, TEntity>>,
                DefaultEntityFrameworkIncludesProvider<Guid, CustomFieldsEntity<TKey, TEntity>>>(false);

            builder.WithRegistration<IEntityQueryOrderByHandler<Guid, CustomFieldsEntity<TKey, TEntity>>,
                DefaultEntityQueryOrderByHandler<Guid, CustomFieldsEntity<TKey, TEntity>>>(false);

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
