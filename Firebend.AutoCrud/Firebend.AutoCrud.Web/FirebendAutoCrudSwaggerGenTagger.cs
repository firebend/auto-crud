using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Firebend.AutoCrud.Web.Attributes;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Firebend.AutoCrud.Web
{
    public static class FirebendAutoCrudSwaggerGenTagger
    {
        public static List<string> TagActionsBy(ApiDescription apiDescription)
        {
            List<string> list;

            if (apiDescription.ActionDescriptor is ControllerActionDescriptor controllerDescriptor)
            {
                list = new List<string>
                {
                    controllerDescriptor.ControllerTypeInfo?.GetCustomAttribute<OpenApiGroupNameAttribute>()?.GroupName ??
                    controllerDescriptor.ControllerTypeInfo?.Namespace?.Split('.').Last() ??
                    apiDescription.RelativePath
                };
            }
            else
            {
                list = new List<string>
                {
                    apiDescription.RelativePath
                };
            }

            return list;
        }
    }
}