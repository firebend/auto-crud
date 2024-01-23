using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.ClassGeneration;
using Firebend.AutoCrud.Generator.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.EntityFramework;

public class EntityFrameworkEntityCrudGenerator : EntityCrudGenerator
{
    public EntityFrameworkEntityCrudGenerator(IDynamicClassGenerator classGenerator, IServiceCollection serviceCollection) : base(classGenerator,
        serviceCollection)
    {
    }

    public EntityFrameworkEntityCrudGenerator(IServiceCollection serviceCollection) : base(serviceCollection)
    {
    }

    public EntityFrameworkEntityCrudGenerator AddEntity<TKey, TEntity>(Action<EntityFrameworkEntityBuilder<TKey, TEntity>> configure)
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        var builder = new EntityFrameworkEntityBuilder<TKey, TEntity>();
        configure(builder);
        Builders.Add(builder);
        return this;
    }
}
