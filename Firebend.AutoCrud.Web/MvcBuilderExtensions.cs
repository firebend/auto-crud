using System.Linq;
using System.Reflection;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Web.Conventions;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddFirebendAutoCrudWeb(this IMvcBuilder builder, IServiceCollection serviceCollection) =>
            builder.ConfigureApplicationPartManager(manager =>
            {
                if (manager.FeatureProviders.Any(fp => fp is FirebendAutoCrudControllerConvention))
                {
                    return;
                }

                manager.FeatureProviders.Insert(0, new FirebendAutoCrudControllerConvention(serviceCollection));
            });

        public static IMvcBuilder AddDefaultResourceAuthorizationRequirements(this IMvcBuilder builder)
        {
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(ReadAllAuthorizationRequirement.DefaultPolicy,
                    policy => policy.Requirements.Add(new ReadAllAuthorizationRequirement()));
                options.AddPolicy(ReadAuthorizationRequirement.DefaultPolicy,
                    policy => policy.Requirements.Add(new ReadAuthorizationRequirement()));
                options.AddPolicy(CreateAuthorizationRequirement.DefaultPolicy,
                    policy => policy.Requirements.Add(new CreateAuthorizationRequirement()));
                options.AddPolicy(CreateMultipleAuthorizationRequirement.DefaultPolicy,
                    policy => policy.Requirements.Add(new CreateMultipleAuthorizationRequirement()));
                options.AddPolicy(UpdateAuthorizationRequirement.DefaultPolicy,
                    policy => policy.Requirements.Add(new UpdateAuthorizationRequirement()));
                options.AddPolicy(DeleteAuthorizationRequirement.DefaultPolicy,
                    policy => policy.Requirements.Add(new DeleteAuthorizationRequirement()));
            });

            return builder;
        }

        public static void AddResourceAuthorizationHandlers(this IMvcBuilder builder)
            => builder.Services.RegisterAllTypes<IAuthorizationHandler>(new []{Assembly.GetEntryAssembly()});
    }
}
