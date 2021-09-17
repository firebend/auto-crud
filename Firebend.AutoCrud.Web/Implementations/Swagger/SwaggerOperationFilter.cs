using System.Collections.Generic;
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

            var endpointMetaData = context
                .ApiDescription
                ?.ActionDescriptor
                ?.EndpointMetadata;

            if (endpointMetaData is null)
            {
                return;
            }

            SanitizeSummaryAndDescription(operation, endpointMetaData);
            AssignOperationId(operation, endpointMetaData);
        }

        private static void AssignOperationId(OpenApiOperation operation, IEnumerable<object> endpointMetaData)
        {
            var operationIdAttribute = endpointMetaData.OfType<OpenApiOperationIdAttribute>().FirstOrDefault();

            if (operationIdAttribute is null)
            {
                return;
            }

            operation.OperationId = operationIdAttribute.OperationId;
        }

        private static void SanitizeSummaryAndDescription(OpenApiOperation operation, IEnumerable<object> endpointMetaData)
        {
            var entityNameAttribute = endpointMetaData.OfType<OpenApiEntityNameAttribute>().FirstOrDefault();

            if (entityNameAttribute is null)
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
