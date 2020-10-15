#region

using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

#endregion

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions
{
    public static class EntityCrudBuilderExtensionsCreate
    {
        public static TBuilder WithCreate<TBuilder>(this TBuilder builder, Type registrationType, Type serviceType)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntityCreateService<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType));
        }

        public static TBuilder WithCreate<TBuilder, TRegistration, TService>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithCreate(typeof(TRegistration), typeof(TService));
        }

        public static TBuilder WithCreate<TBuilder>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            var registrationType = typeof(IEntityCreateService<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType);
            var serviceType = builder.CreateType.MakeGenericType(builder.EntityKeyType, builder.EntityType);

            return builder.WithCreate(registrationType, serviceType);
        }
    }
}