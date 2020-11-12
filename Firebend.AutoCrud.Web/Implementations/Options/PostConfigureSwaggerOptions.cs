using System.Linq;
using Firebend.AutoCrud.Web.Implementations.Swagger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Firebend.AutoCrud.Web.Implementations.Options
{
    public class PostConfigureSwaggerOptions : IPostConfigureOptions<SwaggerGenOptions>
    {
        public void PostConfigure(string name, SwaggerGenOptions options)
        {
            if (options.SwaggerGeneratorOptions.TagsSelector == null ||
                options.SwaggerGeneratorOptions.TagsSelector.Method.Name == "DefaultTagsSelector")
            {
                options.SwaggerGeneratorOptions.TagsSelector = FirebendAutoCrudSwaggerGenTagger.TagActionsBy;
            }

            options.EnableAnnotations();

            if (!options.OperationFilterDescriptors.Any(filter => filter.Type == typeof(SwaggerOperationFilter)))
            {
                options.OperationFilter<SwaggerOperationFilter>();
            }
        }
    }
}
