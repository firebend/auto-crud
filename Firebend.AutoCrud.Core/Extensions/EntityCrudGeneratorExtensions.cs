using Firebend.AutoCrud.Core.Interfaces.Services;
using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Firebend.AutoCrud.Core.Extensions;

public static class EntityCrudGeneratorExtensions
{
    public static IEntityCrudGenerator WithDomainEventContextProvider<TProvider>(this IEntityCrudGenerator generator)
        where TProvider : class, IDomainEventContextProvider
    {
        if (generator.Builders == null)
        {
            return generator;
        }

        foreach (var builder in generator.Builders)
        {
            builder.WithRegistration<IDomainEventContextProvider, TProvider>(replace: true);
        }

        generator.Services.Replace(new ServiceDescriptor(typeof(IDomainEventContextProvider), typeof(TProvider), ServiceLifetime.Scoped));

        return generator;
    }
}
