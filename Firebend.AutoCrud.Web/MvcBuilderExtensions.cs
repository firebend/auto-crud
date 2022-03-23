using System.Linq;
using System.Reflection;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Web.Conventions;
using Firebend.AutoCrud.Web.Implementations.Authorization;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder
            AddFirebendAutoCrudWeb(this IMvcBuilder builder, IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<EntityAuthProvider>();
            return builder.ConfigureApplicationPartManager(manager =>
            {
                if (manager.FeatureProviders.Any(fp => fp is FirebendAutoCrudControllerConvention))
                {
                    return;
                }

                manager.FeatureProviders.Insert(0, new FirebendAutoCrudControllerConvention(serviceCollection));
            });
        }
    }
}
