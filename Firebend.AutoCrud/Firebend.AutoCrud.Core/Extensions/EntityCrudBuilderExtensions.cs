using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Extensions
{
    public static class EntityCrudBuilderExtensions
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
        
        public static TBuilder WithSearch<TBuilder>(this TBuilder builder, Type registrationType, Type serviceType, Type searchType)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithRegistration(registrationType,
                serviceType,
                typeof(IEntitySearchService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, searchType));
        }

        public static TBuilder WithSearch<TBuilder, TRegistration, TService, TSearch>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
            where TSearch : EntitySearchRequest
        {
            return builder.WithSearch(typeof(TRegistration), typeof(TService), typeof(TSearch));
        }

        public static TBuilder WithSearch<TBuilder, TSearch>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
            where TSearch : EntitySearchRequest
        {
            var searchType = typeof(TSearch);
            
            var registrationType = typeof(IEntitySearchService<,,>).MakeGenericType(builder.EntityKeyType, builder.EntityType, searchType);
            var serviceType = builder.SearchType.MakeGenericType(builder.EntityKeyType, builder.EntityType, searchType);

            return builder.WithSearch(registrationType, serviceType, searchType);
        }

        public static TBuilder WithSearch<TBuilder>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithSearch<TBuilder, EntitySearchRequest>();
        }
        
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

        public static TBuilder WithCrud<TBuilder, TSearch>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
            where TSearch : EntitySearchRequest
        {
            return builder
                .WithCreate()
                .WithRead()
                .WithUpdate()
                .WithDelete()
                .WithSearch<TBuilder,TSearch>();
        }

        public static TBuilder WithCrud<TBuilder>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithCrud<TBuilder, EntitySearchRequest>();
        }
    }
}