using Firebend.AutoCrud.Web.Implementations.ApiBehaviors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFirebendApiBehaviors(
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
    }
}
