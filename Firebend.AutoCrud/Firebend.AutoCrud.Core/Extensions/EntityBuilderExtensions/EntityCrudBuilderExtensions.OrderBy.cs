using System;
using System.Linq.Expressions;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models;
using Firebend.AutoCrud.Core.Models.ClassGeneration;

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions
{
    public static class EntityCrudBuilderExtensionsOrderBy
    {
        public static EntityCrudBuilder WithOrderBy<TEntity>(this EntityCrudBuilder builder, (Expression<Func<TEntity, object>>, bool @ascending) orderBy)
        {
            var signature = $"{builder.EntityType.Name}_{builder.EntityName}_OrderBy";

            var iFaceType = typeof(IEntityDefaultOrderByProvider<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType);

            var propertySet = new PropertySet
            {
                Name = nameof(IEntityDefaultOrderByProvider<Guid, FooEntity>.OrderBy),
                Type = typeof(string),
                Value = orderBy
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