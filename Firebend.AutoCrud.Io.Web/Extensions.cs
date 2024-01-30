using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Io.Web.Abstractions;
using Firebend.AutoCrud.Io.Web.Interfaces;
using Firebend.AutoCrud.Web;
using Firebend.AutoCrud.Web.Implementations.Paging;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Io.Web;

public static class Extensions
{
    public static Type IoControllerType<TBuilder, TKey, TEntity, TVersion>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        => typeof(AbstractIoController<,,,,>)
            .MakeGenericType(
                configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType,
                typeof(TVersion),
                configurator.Builder.SearchRequestType,
                configurator.Builder.ExportType);

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
    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithIoControllers<TBuilder, TKey, TEntity, TVersion>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator,
        string entityName = null,
        string entityNamePlural = null,
        string openApiName = null)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
    {
        if (configurator?.Builder?.SearchRequestType == null)
        {
            throw new ArgumentException("No search request type is configured for this controller configurator.");
        }

        if (configurator.Builder?.ExportType == null)
        {
            throw new ArgumentException(
                "No export type configured for this controller configurator. Use IoConfigurator to set mappings.");
        }

        var controllerType = typeof(AbstractIoController<,,,,>);

        var iface = typeof(IEntityExportControllerService<,,,,>).MakeGenericType(configurator.Builder.EntityKeyType,
            configurator.Builder.EntityType,
            typeof(TVersion),
            configurator.Builder.SearchRequestType,
            configurator.Builder.ExportType);

        var impl = typeof(EntityExportControllerService<,,,,>).MakeGenericType(
            configurator.Builder.EntityKeyType,
            configurator.Builder.EntityType,
            typeof(TVersion),
            configurator.Builder.SearchRequestType,
            configurator.Builder.ExportType);

        configurator.Builder.WithRegistration(iface, impl, iface);

        return configurator.WithController(controllerType, controllerType,
            entityName,
            entityNamePlural,
            openApiName,
            configurator.Builder.EntityKeyType,
            configurator.Builder.EntityType,
            typeof(TVersion),
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
    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithMaxExportPageSize<TBuilder, TKey, TEntity, TVersion>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator,
        int pageSize)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
    {
        configurator.Builder.WithRegistrationInstance<IMaxExportPageSize<TKey, TEntity, TVersion>>(
            new DefaultMaxPageSize<TEntity, TKey, TVersion>(pageSize));

        return configurator;
    }

    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddIoAuthorizationPolicy<TBuilder, TKey, TEntity, TVersion>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator, string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        => configurator.AddAuthorizationPolicy(configurator.IoControllerType(),
            policy);
}
