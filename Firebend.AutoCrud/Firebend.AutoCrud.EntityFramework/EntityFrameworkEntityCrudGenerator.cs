using Firebend.AutoCrud.Core.Interfaces.Services.ClassGeneration;
using Firebend.AutoCrud.Generator.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.EntityFramework
{
    public class EntityFrameworkEntityCrudGenerator : EntityCrudGenerator<EntityFrameworkEntityBuilder>
    {
        public EntityFrameworkEntityCrudGenerator(IDynamicClassGenerator classGenerator, IServiceCollection serviceCollection) : base(classGenerator, serviceCollection)
        {
        }

        public EntityFrameworkEntityCrudGenerator(IServiceCollection serviceCollection) : base(serviceCollection)
        {
        }
    }
}