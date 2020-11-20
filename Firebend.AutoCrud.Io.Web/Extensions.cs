using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Io.Web.Abstractions;
using Firebend.AutoCrud.Io.Web.Interfaces;
using Firebend.AutoCrud.Web;

namespace Firebend.AutoCrud.Io.Web
{
    public static class Extensions
    {
        public static ControllerConfigurator<TBuilder, TKey, TEntity> WithIoControllers<TBuilder, TKey, TEntity>(
            this ControllerConfigurator<TBuilder, TKey, TEntity> configurator)
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

            return configurator.WithController(controllerType, controllerType, configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType,
                configurator.Builder.SearchRequestType,
                configurator.Builder.ExportType);
        }
    }
}
