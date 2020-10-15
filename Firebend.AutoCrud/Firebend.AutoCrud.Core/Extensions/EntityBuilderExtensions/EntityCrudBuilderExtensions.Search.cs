using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions
{
    public static class EntityCrudBuilderExtensionsSearch
    {
        public static TBuilder WithSearch<TBuilder>(this TBuilder builder, Type registrationType, Type serviceType, Type searchType)
            where TBuilder : EntityCrudBuilder
        {
            builder.SearchRequestType = searchType;

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
    }
}