using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
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

        public static EntityFrameworkEntityBuilder WithSearchFilter<TEntity>(this EntityFrameworkEntityBuilder builder, Expression<Func<string, TEntity, bool>> filter)
        {
            var signature = $"{builder.EntityType.Name}_{builder.EntityName}_SearchFilter";

            var iFaceType = typeof(IEntityFrameworkFullTextExpressionProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType);

            var propertySet = new PropertySet
            {
                Name = nameof(IEntityFrameworkFullTextExpressionProvider<Guid, FooEntity>.Test),
                Type = typeof(string),
                Value = "filter",
                Override = true
            };
            
            var propertySet1 = new PropertySet<Expression<Func<string, TEntity, bool>>>
            {
                Name = nameof(IEntityFrameworkFullTextExpressionProvider<Guid, FooEntity>.Filter),
                Value = filter,
                Override = true
            };

            return builder.WithDynamicClass(iFaceType, new DynamicClassRegistration
            {
                Interface = iFaceType,
                Properties = new [] { propertySet, propertySet1 },
                Signature = signature,
                Lifetime = ServiceLifetime.Singleton
            });
        }
    }
}