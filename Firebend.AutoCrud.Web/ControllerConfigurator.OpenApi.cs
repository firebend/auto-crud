using System;
using System.Reflection.Emit;
using Firebend.AutoCrud.Web.Attributes;
using Firebend.AutoCrud.Web.Implementations.Options;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Firebend.AutoCrud.Web;

internal static class ControllerConfiguratorStatics
{
    public static readonly object Locker = new();
}

public partial class ControllerConfigurator<TBuilder, TKey, TEntity, TVersion>
{
    public string OpenApiGroupName { get; private set; }
    public string OpenApiEntityName { get; private set; }
    public string OpenApiEntityNamePlural { get; private set; }

    /// <summary>
    /// Specifies the group name to list an entity under for OpenApi and Swagger documentation
    /// </summary>
    /// <param name="openApiGroupName">The group name to use</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers(true)
    ///          .WithOpenApiGroupName("Weather Forecasts")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithOpenApiGroupName(string openApiGroupName)
    {
        OpenApiGroupName = openApiGroupName;

        var (aType, aBuilder) = GetOpenApiGroupAttributeInfo(openApiGroupName);

        AddAttributeToAllControllers(aType, aBuilder);

        AddSwaggerGenOptionConfiguration();

        return this;
    }

    /// <summary>
    /// Specifies the entity name to use fo an entity under in OpenApi and Swagger documentation
    /// </summary>
    /// <param name="name">The entity name to use</param>
    /// <param name="plural">Optional: the entity name to use when a plural is required, automatically pluralized if not provided</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers(true)
    ///          .WithOpenApiGroupName("Weather Forecast")
    /// </code>
    /// </example>
    public ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> WithOpenApiEntityName(string name, string plural = null)
    {
        OpenApiEntityName = name;
        OpenApiEntityNamePlural = plural ?? name.Pluralize();

        var (aType, aBuilder) = GetOpenApiEntityNameAttribute(OpenApiEntityName, OpenApiEntityNamePlural);

        AddAttributeToAllControllers(aType, aBuilder);

        AddSwaggerGenOptionConfiguration();

        return this;
    }

    private void AddSwaggerGenOptionConfiguration()
    {
        if (ControllerConfiguratorCache.IsSwaggerApplied)
        {
            return;
        }

        lock(ControllerConfiguratorStatics.Locker)
        {
            if (ControllerConfiguratorCache.IsSwaggerApplied)
            {
                return;
            }

            Builder.WithServiceCollectionHook(sc =>
                sc.TryAddEnumerable(ServiceDescriptor
                    .Transient<IPostConfigureOptions<SwaggerGenOptions>, PostConfigureSwaggerOptions>()));

            ControllerConfiguratorCache.IsSwaggerApplied = true;
        }
    }


    private (Type attributeType, CustomAttributeBuilder attributeBuilder) GetOpenApiGroupAttributeInfo(
        string openApiName)
    {
        if (string.IsNullOrWhiteSpace(openApiName))
        {
            openApiName = OpenApiGroupName;
        }

        var attributeType = typeof(OpenApiGroupNameAttribute);
        var attributeCtor = attributeType.GetConstructor(new[] { typeof(string) });

        if (attributeCtor == null)
        {
            return default;
        }

        var attributeBuilder = new CustomAttributeBuilder(attributeCtor, new object[] { openApiName });

        return (attributeType, attributeBuilder);
    }

    private (Type attributeType, CustomAttributeBuilder attributeBuilder) GetOpenApiEntityNameAttribute(string name,
        string plural)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            name = OpenApiEntityName;
        }

        if (string.IsNullOrWhiteSpace(plural))
        {
            plural = name;
        }

        var attributeType = typeof(OpenApiEntityNameAttribute);

        var attributeCtor = attributeType.GetConstructor(new[] { typeof(string), typeof(string) });

        if (attributeCtor == null)
        {
            return default;
        }

        var attributeBuilder = new CustomAttributeBuilder(attributeCtor, new object[] { name, plural });

        return (attributeType, attributeBuilder);
    }

    private void AddOpenApiGroupNameAttribute(Type controllerType, string openApiName)
    {
        var (attributeType, attributeBuilder) = GetOpenApiGroupAttributeInfo(openApiName);
        Builder.WithAttribute(controllerType, attributeType, attributeBuilder);
    }

    private void AddOpenApiEntityNameAttribute(Type controllerType, string name, string plural)
    {
        var (attributeType, attributeBuilder) = GetOpenApiEntityNameAttribute(name, plural);
        Builder.WithAttribute(controllerType, attributeType, attributeBuilder);
    }
}
