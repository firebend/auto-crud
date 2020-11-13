using System.Linq;
using Firebend.AutoCrud.Web.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Firebend.AutoCrud.Web.Implementations.Swagger
{
    public class SwaggerOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (string.IsNullOrWhiteSpace(operation.Summary))
            {
                return;
            }

            var entityNameAttribute = context
                .ApiDescription
                ?.ActionDescriptor
                ?.EndpointMetadata
                ?.OfType<OpenApiEntityNameAttribute>()
                .FirstOrDefault();

            if (entityNameAttribute == null)
            {
                return;
            }

            operation.Summary = SanitizeEntityName(operation.Summary, entityNameAttribute);

            foreach (var (_, response) in operation.Responses)
            {
                response.Description = SanitizeEntityName(response.Description, entityNameAttribute);
            }
        }

        private static string SanitizeEntityName(string str, OpenApiEntityNameAttribute entityNameAttribute) => str
            .Replace("{entityName}", entityNameAttribute.Name)
            .Replace("{entityNamePlural}", entityNameAttribute.Plural);
    }
}
