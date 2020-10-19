using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Core.Models.ClassGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions
{
    public static class EntityCrudBuilderExtensionsOrderBy
    {
        public static TBuilder WithOrderBy<TBuilder>(this TBuilder builder, Type type)
            where TBuilder : EntityBuilder
        {
            return builder.WithRegistration(
                typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType),
                type,
                typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType)
            );
        }

        public static EntityCrudBuilder WithOrderBy<T>(this EntityCrudBuilder builder)
        {
            return builder.WithOrderBy(typeof(T));
        }
        
        private static EntityCrudBuilder WithOrderBy<TEntity>(this EntityCrudBuilder builder, (Expression<Func<TEntity, object>>, bool @ascending) orderBy)
        {
            var signature = $"{builder.EntityType.Name}_{builder.EntityName}_OrderBy";

            var iFaceType = typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType);

            var propertySet = new PropertySet
            {
                Name = nameof(IEntityDefaultOrderByProvider<Guid, FooEntity>.OrderBy),
                Type = typeof((Expression<Func<TEntity, object>>, bool @ascending)),
                Value = orderBy
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