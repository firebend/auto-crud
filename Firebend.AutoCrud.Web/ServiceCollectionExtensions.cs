using System.Reflection;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Web.Implementations.ApiBehaviors;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFirebendAutoCrudApiBehaviors(
            this IServiceCollection serviceCollection,
            bool suppressBindingSource = true,
            bool useValidationProblemDetails = true) => serviceCollection.Configure<ApiBehaviorOptions>(o =>
        {
            o.SuppressInferBindingSourcesForParameters = suppressBindingSource;

            if (useValidationProblemDetails)
            {
                o.InvalidModelStateResponseFactory = ValidationProblemDetailsModelStateResponseFactory.InvalidModelStateResponseFactory;
            }
        });

        public static IServiceCollection AddDefaultResourceAuthorizationRequirements(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddAuthorization(options =>
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

            return serviceCollection;
        }

        public static void AddResourceAuthorizationHandlers(this IServiceCollection serviceCollection)
            => serviceCollection.RegisterAllTypes<IAuthorizationHandler>(new[] { Assembly.GetEntryAssembly() },
                ServiceLifetime.Scoped);
    }
}
