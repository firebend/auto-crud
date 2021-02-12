using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Io.Web.Abstractions;
using Firebend.AutoCrud.Io.Web.Interfaces;
using Firebend.AutoCrud.Web;
using Firebend.AutoCrud.Web.Implementations.Paging;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Io.Web
{
    public static class Extensions
    {
        /// <summary>
        /// Specify the max page size to use for Export endpoints
        /// </summary>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers(true, true)
        ///          .WithIoControllers())
        /// </code>
        /// </example>
        public static ControllerConfigurator<TBuilder, TKey, TEntity> WithIoControllers<TBuilder, TKey, TEntity>(
            this ControllerConfigurator<TBuilder, TKey, TEntity> configurator,
            string entityName = null,
            string entityNamePlural = null,
            string openApiName = null)
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            if (configurator?.Builder?.SearchRequestType == null)
            {
                throw new ArgumentException("No search request type is configured for this controller configurator.");
            }

            if (configurator.Builder?.ExportType == null)
            {
                throw new ArgumentException("No export type configured for this controller configurator. Use IoConfigurator to set mappings.");
            }

            var controllerType = typeof(AbstractIoController<,,,>);

            var iface = typeof(IEntityExportControllerService<,,,>).MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType,
                configurator.Builder.SearchRequestType,
                configurator.Builder.ExportType);

            var impl = typeof(AbstractEntityExportControllerService<,,,>).MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType,
                configurator.Builder.SearchRequestType,
                configurator.Builder.ExportType);

            configurator.Builder.WithRegistration(iface, impl, iface);

            return configurator.WithController(controllerType, controllerType,
                entityName,
                entityNamePlural,
                openApiName,
                configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType,
                configurator.Builder.SearchRequestType,
                configurator.Builder.ExportType);
        }

        /// <summary>
        /// Specify the max page size to use for Export endpoints
        /// </summary>
        /// <param name="pageSize">The max page size to use</param>
        /// <example>
        /// <code>
        /// forecast.WithDefaultDatabase("Samples")
        ///      .WithCollection("WeatherForecasts")
        ///      .WithFullTextSearch()
        ///      .AddCrud()
        ///      .AddControllers(controllers => controllers
        ///          .WithAllControllers(true, true)
        ///          .WithMaxExportPageSize(100))
        /// </code>
        /// </example>
        public static ControllerConfigurator<TBuilder, TKey, TEntity> WithMaxExportPageSize<TBuilder, TKey, TEntity>(
            this ControllerConfigurator<TBuilder, TKey, TEntity> configurator,
            int pageSize)
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            configurator.Builder.WithRegistrationInstance<IMaxExportPageSize<TKey, TEntity>>(new DefaultMaxPageSize<TEntity, TKey>(pageSize));

            return configurator;
        }
    }
}
