using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Models.Searching;

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions
{
    public static class EntityCrudBuilderExtensions
    {
        public static TBuilder WithCrud<TBuilder, TSearch>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
            where TSearch : EntitySearchRequest
        {
            return builder
                .WithCreate()
                .WithRead()
                .WithUpdate()
                .WithDelete()
                .WithSearch<TBuilder, TSearch>();
        }

        public static TBuilder WithCrud<TBuilder>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return builder.WithCrud<TBuilder, EntitySearchRequest>();
        }

        public static TBuilder AsBuilder<TBuilder>(this EntityCrudBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return (TBuilder) builder;
        }
    }
}