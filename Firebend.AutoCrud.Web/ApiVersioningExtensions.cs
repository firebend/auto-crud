using System;
using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Firebend.AutoCrud.Web;

public static class ApiVersioningExtensions
{
    public static IServiceCollection AddAutoCrudApiVersioning(this IServiceCollection services,
        Func<ApiVersionDescription, string> getTitle,
        IEnumerable<Type> versions,
        Action<ApiVersioningOptions> configureVersioningOptions = null,
        Action<ApiExplorerOptions> configureExplorerOptions = null,
        Action<SwaggerGenOptions> configureSwaggerGenOptions = null)
    {
        if (versions.Any(x => !x.IsAssignableTo(typeof(IApiVersion))))
        {
            throw new Exception("All versions must implement IApiVersion");
        }

        foreach (var version in versions)
        {
            services.AddSingleton(typeof(IApiVersion), version);
        }

        return services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = false;
                o.AssumeDefaultVersionWhenUnspecified = true;

                var provider = services.BuildServiceProvider();
                var versions = provider.GetServices<IApiVersion>();

                var allControllerTypes = services
                    .Where(x => x.ServiceType.IsAssignableTo(typeof(IAutoCrudController)))
                    .Select(x => x.ServiceType)
                    .ToList();

                foreach (var version in versions.OrderBy(x => x.Version).ThenBy(x => x.MinorVersion))
                {
                    var type = typeof(AbstractEntityControllerBase<>).MakeGenericType(version.GetType());
                    var controllers = allControllerTypes.Where(x => x.IsAssignableTo(type)).ToList();

                    foreach (var controller in controllers)
                    {
                        var futureVersions = versions
                            .Where(x => x.Version > version.Version
                                        || (x.Version == version.Version && x.MinorVersion > version.MinorVersion))
                            .ToList();

                        var controllerType = controller;
                        while (!controllerType.IsGenericType || !controllerType.GetGenericArguments()
                                   .Any(x => x.IsAssignableTo(typeof(IApiVersion))))
                        {
                            controllerType = controllerType.BaseType;
                        }

                        var futureVersionTypes = futureVersions.Select(fv =>
                        {
                            var genericArgs = controllerType.GetGenericArguments();
                            return controllerType.GetGenericTypeDefinition().MakeGenericType(genericArgs
                                .Select(arg =>
                                    arg.IsAssignableTo(typeof(IApiVersion)) ? fv.GetType() : arg).ToArray());
                        });


                        if (futureVersionTypes.Any(type => allControllerTypes.Any(y => y.IsAssignableTo(type))))
                        {
                            o.Conventions.Controller(controller)
                                .HasDeprecatedApiVersion(new ApiVersion(version.Version, version.MinorVersion));
                        }
                        else
                        {
                            o.Conventions.Controller(controller)
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

                configureSwaggerGenOptions?.Invoke(o);
            });
    }
}
