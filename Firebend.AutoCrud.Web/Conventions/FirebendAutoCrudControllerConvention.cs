using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web.Conventions
{
    public class FirebendAutoCrudControllerConvention : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly IServiceCollection _services;

        public FirebendAutoCrudControllerConvention(IServiceCollection services)
        {
            _services = services;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var baseControllerType = typeof(ControllerBase);

            var controllers = _services
                .Where(x => baseControllerType.IsAssignableFrom(x.ServiceType))
                .ToArray();

            foreach (var descriptor in controllers)
            {
                feature.Controllers.Add(descriptor.ServiceType.GetTypeInfo());
            }
        }
    }
}
