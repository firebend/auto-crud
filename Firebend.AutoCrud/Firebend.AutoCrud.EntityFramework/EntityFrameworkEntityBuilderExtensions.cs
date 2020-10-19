using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Core.Models.ClassGeneration;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.EntityFramework
{
    public static class EntityFrameworkEntityBuilderExtensions
    {
        public static EntityFrameworkEntityBuilder WithSearchFilter(this EntityFrameworkEntityBuilder builder, Type type)
        {
            return builder.WithRegistration(
                typeof(IEntityFrameworkFullTextExpressionProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType),
                type,
                typeof(IEntityFrameworkFullTextExpressionProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType)
            );
        }

        public static EntityFrameworkEntityBuilder WithSearchFilter<T>(this EntityFrameworkEntityBuilder builder)
        {
            return builder.WithSearchFilter(typeof(T));
        }

        private static EntityFrameworkEntityBuilder WithSearchFilter<TEntity>(this EntityFrameworkEntityBuilder builder, Expression<Func<string, TEntity, bool>> filter)
        {
            var signature = $"{builder.EntityType.Name}_{builder.EntityName}_SearchFilter";

            var iFaceType = typeof(IEntityFrameworkFullTextExpressionProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType);

            var propertySet = new PropertySet
            {
                Name = nameof(IEntityFrameworkFullTextExpressionProvider<Guid, FooEntity>.Filter),
                Type = typeof(Expression<Func<string, TEntity, bool>>),
                Value = filter,
                Override = true
            };

            return builder.WithDynamicClass(iFaceType, new DynamicClassRegistration
            {
                Interface = iFaceType,
                Properties = new [] { propertySet },
                Signature = signature,
                Lifetime = ServiceLifetime.Singleton
            });
        }
    }
}