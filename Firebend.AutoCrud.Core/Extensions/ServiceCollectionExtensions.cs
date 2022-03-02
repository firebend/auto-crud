using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static void RegisterAllTypes<T>(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        var typesFromAssemblies =
            assemblies.SelectMany(a
                => a.DefinedTypes.Where(x => x.GetInterfaces().Contains(typeof(T))));

        foreach (var type in typesFromAssemblies)
        {
            services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
        }
    }
}
