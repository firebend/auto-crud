using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Configurators;

namespace Firebend.AutoCrud.Core.Extensions.EntityBuilderExtensions
{
    public static class EntityBuilderConfiguratorExtensions
    {
        public static TBuilder AddCrud<TBuilder>(this TBuilder builder,
            Action<EntityCrudConfigurator<TBuilder>> configure)
            where TBuilder : EntityCrudBuilder
        {
            var config = new EntityCrudConfigurator<TBuilder>(builder);
            configure(config);
            return builder;
        }
        
        public static TBuilder AddCrud<TBuilder>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            var config = new EntityCrudConfigurator<TBuilder>(builder);
            config.WithCrud();
            return builder;
        }

        public static TBuilder AddDomainEvents<TBuilder>(this TBuilder builder,
            Action<DomainEventsConfigurator<TBuilder>> configure) where TBuilder : EntityBuilder
        {
            var config = new DomainEventsConfigurator<TBuilder>(builder);
            configure(config);
            return builder;
        }
    }
}