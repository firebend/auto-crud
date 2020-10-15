#region

using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;

#endregion

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions
{
    public static class EntityCrudBuilderExtensionsUpdate
    {
        public static TBuilder WithUpdate<TBuilder>(this TBuilder builder, Type registrationType, Type serviceType)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntityUpdateService<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType));
        }

        public static TBuilder WithUpdate<TBuilder, TRegistration, TService>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithUpdate(typeof(TRegistration), typeof(TService));
        }

        public static TBuilder WithUpdate<TBuilder>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            var registrationType = typeof(IEntityUpdateService<,>).MakeGenericType(builder.EntityKeyType, builder.EntityType);
            var serviceType = builder.UpdateType.MakeGenericType(builder.EntityKeyType, builder.EntityType);

            return builder.WithUpdate(registrationType, serviceType);
        }
    }
}