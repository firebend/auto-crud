using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Abstractions.Services;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.CustomFields.EntityFramework.Abstractions;

namespace Firebend.AutoCrud.CustomFields.EntityFramework
{
    public static class Extensions
    {
        public static EntityCrudBuilder<TKey, TEntity> AddCustomFields<TKey, TEntity>(
            this EntityCrudBuilder<TKey, TEntity> builder)
            where TKey : struct
            where TEntity : class, IEntity<TKey>, ICustomFieldsEntity<TKey>
        {
            builder.WithRegistration<ICustomFieldsCreateService<TKey, TEntity>, AbstractCustomFieldsAlterService<TKey, TEntity>>();
            builder.WithRegistration<ICustomFieldsDeleteService<TKey, TEntity>, AbstractCustomFieldsAlterService<TKey, TEntity>>();
            builder.WithRegistration<ICustomFieldsUpdateService<TKey, TEntity>, AbstractCustomFieldsAlterService<TKey, TEntity>>();

            builder.WithRegistration<ICustomFieldsSearchService<TKey, TEntity>, AbstractEfCustomFieldSearchService<TKey, TEntity>>();

            return builder;
        }
    }
}
