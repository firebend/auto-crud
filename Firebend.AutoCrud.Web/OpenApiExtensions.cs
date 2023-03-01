using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Firebend.AutoCrud.Web;

public static class OpenApiExtensions
{
    /// <summary>
    /// Enables OpenAPI documentation for the AutoCrud controllers
    /// </summary>
    /// <param name="getTitle">A func that takes an ApiVersionDescription object and returns the title for the swagger doc.</param>
    /// <param name="markDeprecated">If true, mark controllers that have a matching later version as deprecated.</param>
    /// <param name="forwardNonDeprecatedEndpoints">If true, endpoints that don't have a matching later version will appear in the swagger doc for later versions.</param>
    /// <param name="configureVersioningOptions">Additional configuration for ApiVersioningOptions</param>
    /// <param name="configureExplorerOptions">Additional configuration for ApiExplorerOptions</param>
    /// <param name="configureSwaggerGenOptions">Additional configuration for SwaggerGenOptions</param>
    public static IServiceCollection AddAutoCrudOpenApi(this IServiceCollection services,
        Func<ApiVersionDescription, string> getTitle,
        bool forwardNonDeprecatedEndpoints = false,
        Action<ApiVersioningOptions> configureVersioningOptions = null,
        Action<ApiExplorerOptions> configureExplorerOptions = null,
        Action<SwaggerGenOptions> configureSwaggerGenOptions = null)
    {
        return services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = false;
                o.AssumeDefaultVersionWhenUnspecified = true;

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

                if (forwardNonDeprecatedEndpoints)
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
                                                        || ($"v{v}".CompareTo(docName) < 0 && !apiDesc.IsDeprecated());

                        return versions.DeclaredApiVersions.Any()
                            ? versions.DeclaredApiVersions.Any(Predicate)
                            : versions.ImplementedApiVersions.Any(Predicate);
                    };
                }

                configureSwaggerGenOptions?.Invoke(o);
            });
    }
}

