using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Core.Models.ClassGeneration;
using Firebend.AutoCrud.EntityFramework.Interfaces;

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
        
        //todo: setting this to private until i can figure out why it doesn't work JMA 10/19/2020
        private static EntityFrameworkEntityBuilder WithSearchFilter<TEntity>(this EntityFrameworkEntityBuilder builder, Expression<Func<string, TEntity, bool>> filter)
        {
            var signature = $"{builder.EntityType.Name}_{builder.EntityName}_SearchFilter";

            var iFaceType = typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType);

            var propertySet = new PropertySet
            {
                Name = nameof(IEntityFrameworkFullTextExpressionProvider<Guid, FooEntity>.Filter),
                Type = typeof(Expression<Func<string, TEntity, bool>>),
                Value = filter,
                Override = true
            };

            return builder.WithDynamicClass(new DynamicClassRegistration
            {
                Interface = iFaceType,
                Properties = new [] { propertySet },
                Signature = signature
            });
        }
    }
}