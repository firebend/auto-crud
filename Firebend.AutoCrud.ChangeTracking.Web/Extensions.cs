using System;
using System.Reflection.Emit;
using Firebend.AutoCrud.ChangeTracking.Web.Abstractions;
using Firebend.AutoCrud.ChangeTracking.Web.Implementations.Authorization;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.ChangeTracking.Web;

public static class Extensions
{
    public static ControllerConfigurator<TBuilder, TKey, TEntity> WithChangeTrackingControllers<TBuilder, TKey,
        TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator,
        string entityName = null,
        string entityNamePlural = null,
        string openApiName = null)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        var controller = typeof(AbstractChangeTrackingReadController<,,>)
            .MakeGenericType(configurator.Builder.EntityKeyType,
                configurator.Builder.EntityType,
                configurator.ReadViewModelType);

        return configurator.WithController(controller, controller, entityName, entityNamePlural, openApiName);
    }

    /// <summary>
    /// Adds resource authorization to change tracking read requests using the abstract change tracking controller
    /// </summary>
    /// <param name="policy">The resource authorization policy</param>
    /// <example>
    /// <code>
    /// forecast.WithDefaultDatabase("Samples")
    ///      .WithCollection("WeatherForecasts")
    ///      .WithFullTextSearch()
    ///      .AddCrud()
    ///      .AddControllers(controllers => controllers
    ///          .WithAllControllers()
    ///          .WithChangeTrackingControllers()
    ///          .AddChangeTrackingResourceAuthorization()
    /// </code>
    /// </example>
    public static ControllerConfigurator<TBuilder, TKey, TEntity> AddChangeTrackingResourceAuthorization<TBuilder,
        TKey,
        TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator,
        string policy = ChangeTrackingAuthorizationRequirement.DefaultPolicy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        => AddResourceAuthorization(configurator, typeof(AbstractChangeTrackingReadController<,,>)
                .MakeGenericType(configurator.Builder.EntityKeyType,
                    configurator.Builder.EntityType,
                    configurator.ReadViewModelType),
            typeof(EntityChangeTrackingAuthorizationFilter<TKey, TEntity>), policy);

    public static IMvcBuilder AddDefaultChangeTrackingResourceAuthorizationRequirement(this IMvcBuilder builder)
    {
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(ChangeTrackingAuthorizationRequirement.DefaultPolicy,
                policy => policy.Requirements.Add(new ChangeTrackingAuthorizationRequirement()));
        });

        return builder;
    }

    private static ControllerConfigurator<TBuilder, TKey, TEntity> AddResourceAuthorization<TBuilder, TKey,
        TEntity>(
        this ControllerConfigurator<TBuilder, TKey, TEntity> configurator, Type type, Type filterType,
        string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
    {
        var (attributeType, attributeBuilder) =
            GetResourceAuthorizationAttributeInfo(filterType, policy);
        configurator.Builder.WithAttribute(type, attributeType, attributeBuilder);
        return configurator;
    }

    private static (Type attributeType, CustomAttributeBuilder attributeBuilder)
        GetResourceAuthorizationAttributeInfo(
            Type filterType,
            string policy)
    {
        var authCtor = filterType.GetConstructor(new[] {typeof(string)});

        if (authCtor == null)
        {
            return default;
        }

        var args = new object[] {policy};
        return (filterType,
            new CustomAttributeBuilder(authCtor, args));
    }
}
