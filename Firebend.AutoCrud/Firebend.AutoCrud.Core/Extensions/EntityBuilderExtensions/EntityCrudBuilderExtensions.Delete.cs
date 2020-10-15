using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions
{
    public static class EntityCrudBuilderExtensionsDelete
    {
        public static TBuilder WithDelete<TBuilder>(this TBuilder builder, Type registrationType, Type serviceType)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntityDeleteService<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType));
        }

        public static TBuilder WithDelete<TBuilder, TRegistration, TService>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithDelete(typeof(TRegistration), typeof(TService));
        }

        public static TBuilder WithDelete<TBuilder>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            var registrationType = typeof(IEntityDeleteService<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType);

            var deleteType = typeof(IActiveEntity).IsAssignableFrom(builder.EntityType)
                ? builder.SoftDeleteType
                : builder.DeleteType;

            var serviceType = deleteType.MakeGenericType(builder.EntityKeyType, builder.EntityType);

            return builder.WithDelete(registrationType, serviceType);
        }
    }
}