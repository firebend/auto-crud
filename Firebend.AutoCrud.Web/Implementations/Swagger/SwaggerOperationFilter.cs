using System.Collections.Generic;
using System.Linq;
using Firebend.AutoCrud.Web.Attributes;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
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

            var entityNameAttribute = endpointMetaData.OfType<OpenApiEntityNameAttribute>().FirstOrDefault();

            SanitizeSummaryAndDescription(operation, entityNameAttribute);
            AssignOperationId(operation, endpointMetaData, context, entityNameAttribute);
        }

        private static void AssignOperationId(OpenApiOperation operation,
            IEnumerable<object> endpointMetaData,
            OperationFilterContext operationFilterContext,
            OpenApiEntityNameAttribute openApiEntityNameAttribute)
        {
            var operationIdAttribute = endpointMetaData.OfType<OpenApiOperationIdAttribute>().FirstOrDefault();

            if (operationIdAttribute is not null)
            {
                operation.OperationId = operationIdAttribute.OperationId;
            }
            else
            {
                var opId = GetAutoCrudControllerOperationId(operationFilterContext, openApiEntityNameAttribute);

                if (!string.IsNullOrWhiteSpace(opId))
                {
                    operation.OperationId = opId;
                }
            }

        }

        private static string GetAutoCrudControllerOperationId(OperationFilterContext operationFilterContext, OpenApiEntityNameAttribute openApiEntityNameAttribute)
        {
            if (string.IsNullOrWhiteSpace(openApiEntityNameAttribute?.Name))
            {
                return null;
            }

            if (operationFilterContext?.ApiDescription?.ActionDescriptor is not ControllerActionDescriptor controllerDescriptor)
            {
                return null;
            }

            if (controllerDescriptor.ControllerTypeInfo is null)
            {
                return null;
            }

            var autoCrudType = typeof(IAutoCrudController);
            var isAssignable = false;

            try
            {
                isAssignable = autoCrudType.IsAssignableFrom(controllerDescriptor.ControllerTypeInfo);
            }
            catch
            {
                // ignored
            }

            if (isAssignable is false)
            {
                return null;
            }

            var entityName = openApiEntityNameAttribute.Name.Replace(" ", null);
            var opId = $"{entityName}{controllerDescriptor.ActionName}";

            return opId;
        }

        private static void SanitizeSummaryAndDescription(OpenApiOperation operation, OpenApiEntityNameAttribute entityNameAttribute)
        {
            if (entityNameAttribute is null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(operation.Summary))
            {
                operation.Summary = SanitizeEntityName(operation.Summary, entityNameAttribute);
            }

            foreach (var (_, response) in operation.Responses)
            {
                if (string.IsNullOrWhiteSpace(response.Description))
                {
                    continue;
                }

                response.Description = SanitizeEntityName(response.Description, entityNameAttribute);
            }
        }

        private static string SanitizeEntityName(string str, OpenApiEntityNameAttribute entityNameAttribute)
        {
            if (!string.IsNullOrWhiteSpace(entityNameAttribute.Name))
            {
                str = str.Replace("{entityName}", entityNameAttribute.Name);
            }

            if (!string.IsNullOrWhiteSpace(entityNameAttribute.Plural))
            {
                str = str.Replace("{entityNamePlural}", entityNameAttribute.Plural);
            }

            return str;
        }
    }
}
