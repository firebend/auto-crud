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
            builder.WithRegistration<IDbContextProvider<Guid, EfCustomFieldsModel<TKey, TEntity>>,
                AbstractCustomFieldsDbContextProvider<TKey, TEntity>>();

            builder.WithRegistration<IEntityFrameworkIncludesProvider<Guid, EfCustomFieldsModel<TKey, TEntity>>,
                DefaultEntityFrameworkIncludesProvider<Guid, EfCustomFieldsModel<TKey, TEntity>>>(false);

            builder.WithRegistration<IEntityFrameworkDbUpdateExceptionHandler<Guid, EfCustomFieldsModel<TKey, TEntity>>,
                DefaultEntityFrameworkDbUpdateExceptionHandler<Guid, EfCustomFieldsModel<TKey, TEntity>>>(false);

            builder.WithRegistration<ICustomFieldsCreateService<TKey, TEntity>,
                AbstractEfCustomFieldsCreateService<TKey, TEntity>>(false);

            builder.WithRegistration<IEntityFrameworkCreateClient<Guid, EfCustomFieldsModel<TKey, TEntity>>,
                EntityFrameworkCreateClient<Guid, EfCustomFieldsModel<TKey, TEntity>>>(false);

            builder.WithRegistration<IEntityFrameworkUpdateClient<Guid, EfCustomFieldsModel<TKey, TEntity>>,
                EntityFrameworkUpdateClient<Guid, EfCustomFieldsModel<TKey, TEntity>>>(false);

            builder.WithRegistration<ICustomFieldsUpdateService<TKey, TEntity>,
                AbstractEfCustomFieldsUpdateService<TKey, TEntity>>(false);

            builder.WithRegistration<IEntityFrameworkDeleteClient<Guid, EfCustomFieldsModel<TKey, TEntity>>,
                EntityFrameworkDeleteClient<Guid, EfCustomFieldsModel<TKey, TEntity>>>(false);

            builder.WithRegistration<ICustomFieldsDeleteService<TKey, TEntity>,
                AbstractEfCustomFieldsDeleteService<TKey, TEntity>>(false);

            builder.WithRegistration<IEntityFrameworkQueryClient<Guid, EfCustomFieldsModel<TKey, TEntity>>,
                EntityFrameworkQueryClient<Guid, EfCustomFieldsModel<TKey, TEntity>>>();

            builder.WithRegistration<IEntityFrameworkIncludesProvider<Guid, EfCustomFieldsModel<TKey, TEntity>>,
                DefaultEntityFrameworkIncludesProvider<Guid, EfCustomFieldsModel<TKey, TEntity>>>(false);

            builder.WithRegistration<IEntityQueryOrderByHandler<Guid, EfCustomFieldsModel<TKey, TEntity>>,
                DefaultEntityQueryOrderByHandler<Guid, EfCustomFieldsModel<TKey, TEntity>>>(false);

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
