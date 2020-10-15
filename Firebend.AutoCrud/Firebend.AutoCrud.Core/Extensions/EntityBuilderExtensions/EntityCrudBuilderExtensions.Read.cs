using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions
{
    public static class EntityCrudBuilderExtensionsRead
    {
        public static TBuilder WithRead<TBuilder>(this TBuilder builder, Type registrationType, Type serviceType)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntityReadService<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType));
        }

        public static TBuilder WithRead<TBuilder, TRegistration, TService>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithRead(typeof(TRegistration), typeof(TService));
        }

        public static TBuilder WithRead<TBuilder>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            var registrationType = typeof(IEntityReadService<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType);
            var serviceType = builder.ReadType.MakeGenericType(builder.EntityKeyType, builder.EntityType);

            return builder.WithRead(registrationType, serviceType);
        }
    }
}