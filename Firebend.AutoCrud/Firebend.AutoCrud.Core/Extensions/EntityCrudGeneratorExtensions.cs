using Firebend.AutoCrud.Core.Interfaces.Services;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firebend.AutoCrud.Core.Extensions
{
    public static class EntityCrudGeneratorExtensions
    {
        public static IEntityCrudGenerator WithDomainEventContextProvider<TProvider>(this IEntityCrudGenerator generator)
            where TProvider : class, IDomainEventContextProvider
        {
            generator.ServiceCollection.TryAddScoped<IDomainEventContextProvider, TProvider>();

            if (generator.Builders != null)
            {
                foreach (var builder in generator.Builders)
                {
                    builder.WithRegistration<IDomainEventContextProvider, TProvider>();
                }
            }
            
            return generator;
        }
    }
}