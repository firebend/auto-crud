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

public static class OpenApiExtensions
{
    public static IServiceCollection AddAutoCrudOpenApi(this IServiceCollection services,
        Func<ApiVersionDescription, string> getTitle,
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

                        return ct;
                    })
                    .ToList();

                var versions = allControllerTypes
                    .Select(x => x.GetGenericArguments().First(y => y.IsAssignableTo(typeof(IApiVersion))))
                    .Distinct()
                    .Select(x => (IApiVersion)Activator.CreateInstance(x))
                    .ToList();

                Console.WriteLine(string.Join(", ", versions.Select(x => x.Name)));

                var deprecated = new List<string>();
                foreach (var version in versions.OrderBy(x => x.Version).ThenBy(x => x.MinorVersion))
                {
                    var baseType = typeof(AbstractEntityControllerBase<>).MakeGenericType(version.GetType());

                    // TODO TS: we will have to do something other than isassignable here, either use the url or match the controller by type names somehow
                    var controllerTypes = allControllerTypes.Where(x => x.IsAssignableTo(baseType))
                        .ToList();



                    foreach (var controllerType in controllerTypes)
                    {
                        var futureVersions = versions
                            .Where(x => x.Version > version.Version
                                        || (x.Version == version.Version && x.MinorVersion > version.MinorVersion))
                            .ToList();

                        var futureVersionTypes = futureVersions.Select(fv =>
                        {
                            var genericArgs = controllerType.GetGenericArguments();
                            return controllerType.GetGenericTypeDefinition().MakeGenericType(genericArgs
                                .Select(arg =>
                                    arg.IsAssignableTo(typeof(IApiVersion)) ? fv.GetType() : arg).ToArray());
                        });

                        Console.WriteLine("!!!");
                        PrintType(controllerType);
                        Console.WriteLine("_______________");
                        foreach (var futureVersionType in futureVersionTypes)
                        {
                            PrintType(futureVersionType);
                            Console.WriteLine(allControllerTypes.Any(y => y.IsAssignableTo(futureVersionType)) ? "TRUE" : "FALSE");

                        }

                        deprecated.Add(string.Join("\n", futureVersionTypes.Select(x => x.Name)));

                        if (futureVersionTypes.Any(t => allControllerTypes.Any(y => y.IsAssignableTo(t))))
                        {
                            deprecated.Add(controllerType + " is deprecated");
                            o.Conventions.Controller(controllerType)
                                .HasDeprecatedApiVersion(new ApiVersion(version.Version, version.MinorVersion));
                        }
                        else
                        {
                            o.Conventions.Controller(controllerType)
                                .HasApiVersion(new ApiVersion(version.Version, version.MinorVersion));
                        }
                    }


                }

                throw new Exception(string.Join("\n", deprecated));
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

                o.SwaggerGeneratorOptions.DocInclusionPredicate = (docName, apiDesc) =>
                {
                    var versions = apiDesc.ActionDescriptor
                        .GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

                    // if (apiDesc.RelativePath.Contains("api/v1/mongo-person/validate"))
                    // {
                    //     throw new Exception(System.Text.Json.JsonSerializer.Serialize(versions.DeprecatedApiVersions.Select(x => x.MajorVersion)));
                    // }

                    if (versions == null)
                    {
                        return true;
                    }

                    bool Predicate(ApiVersion v) => $"v{v}" == docName
                                                    || $"v{v.MajorVersion}" == docName
                                                    || $"v{v}".CompareTo(docName) < 0 && !apiDesc.IsDeprecated();

                    if (versions.DeclaredApiVersions.Any())
                    {
                        return versions.DeclaredApiVersions.Any(Predicate);
                    }

                    return versions.ImplementedApiVersions.Any(Predicate);
                };

                configureSwaggerGenOptions?.Invoke(o);
            });
    }

    public static void PrintType(Type t)
    {
        var a = new StringBuilder();
        if (t.IsGenericType)
        {

            a.Append(t.Name.Substring(0, t.Name.IndexOf("`", StringComparison.Ordinal)));
            a.Append("<");
            var args = t.GetGenericArguments();
            for (var i = 0; i < args.Length; i++)
            {
                a.Append(args[i]);
                if (i < args.Length - 1)
                {
                    a.Append(", ");
                }
            }
            a.Append(">");
        }
        else
        {
            a.Append(t.Name);
        }
        Console.WriteLine(a.ToString());
    }
}

