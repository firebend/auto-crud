using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Firebend.AutoCrud.Web;

public class TypeWithOriginalType
{
    public Type Type { get; set; }
    public Type OriginalType { get; set; }
}

public static class OpenApiExtensions
{
    /// <summary>
    /// Enables OpenAPI documentation for the AutoCrud controllers
    /// </summary>
    /// <param name="getTitle">A func that takes an ApiVersionDescription object and returns the title for the swagger doc.</param>
    /// <param name="markDeprecated">If true, mark controllers that have a matching later version as deprecated.</param>
    /// <param name="forwardNonSupersededEndpoints">If true, endpoints that don't have a matching later version will appear in the swagger doc for later versions.</param>
    /// <param name="configureVersioningOptions">Additional configuration for ApiVersioningOptions</param>
    /// <param name="configureExplorerOptions">Additional configuration for ApiExplorerOptions</param>
    /// <param name="configureSwaggerGenOptions">Additional configuration for SwaggerGenOptions</param>
    public static IServiceCollection AddAutoCrudOpenApi(this IServiceCollection services,
        Func<ApiVersionDescription, string> getTitle,
        bool markDeprecated = true,
        bool forwardNonSupersededEndpoints = true,
        Action<ApiVersioningOptions> configureVersioningOptions = null,
        Action<ApiExplorerOptions> configureExplorerOptions = null,
        Action<SwaggerGenOptions> configureSwaggerGenOptions = null)
    {
        return services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = false;
                o.AssumeDefaultVersionWhenUnspecified = true;

                var allControllerTypes = services
                    .Where(x => x.ServiceType.IsAssignableTo(typeof(IAutoCrudController)))
                    .Select(x => x.ServiceType)
                    .Select(x =>
                    {
                        var ct = x;
                        while (!ct.IsGenericType || !ct.GetGenericArguments()
                                   .Any(ct => ct.IsAssignableTo(typeof(IApiVersion))))
                        {
                            ct = ct.BaseType;
                        }

                        return new TypeWithOriginalType { Type = ct, OriginalType = x };
                    })
                    .ToList();

                var versions = allControllerTypes
                    .Select(x => x.Type.GetGenericArguments().First(y => y.IsAssignableTo(typeof(IApiVersion))))
                    .Distinct()
                    .Select(x => (IApiVersion)Activator.CreateInstance(x))
                    .ToList();

                foreach (var version in versions.OrderBy(x => x.Version).ThenBy(x => x.MinorVersion))
                {
                    var baseType = typeof(AbstractEntityControllerBase<>).MakeGenericType(version.GetType());

                    var controllerTypes = allControllerTypes.Where(x => x.Type.IsAssignableTo(baseType)).ToList();

                    foreach (var controllerType in controllerTypes)
                    {
                        var futureVersions = versions
                            .Where(x => x.Version > version.Version
                                        || (x.Version == version.Version && x.MinorVersion > version.MinorVersion))
                            .ToList();

                        var futureVersionTypes = futureVersions.Select(fv =>
                        {
                            var genericArgs = controllerType.Type.GetGenericArguments();
                            return controllerType.Type.GetGenericTypeDefinition().MakeGenericType(genericArgs
                                .Select(arg =>
                                    arg.IsAssignableTo(typeof(IApiVersion)) ? fv.GetType() : arg).ToArray());
                        });

                        if (markDeprecated && futureVersionTypes.Any(t => allControllerTypes.Any(y => ControllerTypesMatch(y.Type, t))))
                        {
                            o.Conventions.Controller(controllerType.OriginalType)
                                .HasDeprecatedApiVersion(new ApiVersion(version.Version, version.MinorVersion));
                        }
                        else
                        {
                            o.Conventions.Controller(controllerType.OriginalType)
                                .HasApiVersion(new ApiVersion(version.Version, version.MinorVersion));
                        }
                    }
                }
                configureVersioningOptions?.Invoke(o);
            })
            .AddVersionedApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;

                configureExplorerOptions?.Invoke(o);
            }).AddSwaggerGen(o =>
            {
                var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    o.SwaggerDoc(
                        description.GroupName,
                        new OpenApiInfo { Title = getTitle(description), Version = description.ApiVersion.ToString() });
                }

                if (forwardNonSupersededEndpoints)
                {
                    o.SwaggerGeneratorOptions.DocInclusionPredicate = (docName, apiDesc) =>
                    {
                        var versions = apiDesc.ActionDescriptor
                            .GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

                        if (versions == null || versions.IsApiVersionNeutral)
                        {
                            return true;
                        }

                        bool Predicate(ApiVersion v) => $"v{v}" == docName
                                                        || $"v{v.MajorVersion}" == docName
                                                        || $"v{v}".CompareTo(docName) < 0 && !apiDesc.IsDeprecated();

                        return versions.DeclaredApiVersions.Any()
                            ? versions.DeclaredApiVersions.Any(Predicate)
                            : versions.ImplementedApiVersions.Any(Predicate);
                    };

                    o.ResolveConflictingActions(actions =>
                    {
                        if (actions.All(apiDesc => apiDesc.ActionDescriptor
                                .GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit)
                                .IsApiVersionNeutral))
                        {
                            return actions.First();
                        }

                        throw new Exception(
                            $"Conflicting actions: {string.Join(", ", actions.Select(x => x.ActionDescriptor.DisplayName))}");
                    });
                }

                configureSwaggerGenOptions?.Invoke(o);
            });
    }

    private static bool ControllerTypesMatch(Type typeA, Type typeB)
    {
        var defA = typeA.GetGenericTypeDefinition();
        var defB = typeB.GetGenericTypeDefinition();

        if (defA != defB)
        {
            return false;
        }

        var argsA = typeA.GetGenericArguments();
        var argsB = typeB.GetGenericArguments();

        // TKey, TEntity, and TVersion are all the same
        return argsA[0] == argsB[0] && argsA[1] == argsB[1] && argsA[2] == argsB[2];
    }
}

