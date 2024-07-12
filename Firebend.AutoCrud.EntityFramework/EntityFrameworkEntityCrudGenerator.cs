using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.ClassGeneration;
using Firebend.AutoCrud.Generator.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.EntityFramework;

public class EntityFrameworkEntityCrudGenerator : EntityCrudGenerator
{
    public Action<IServiceProvider, DbContextOptionsBuilder> DbContextOptionsBuilder { get; }

    public Type DbContextType { get; }
    public bool UsePooled { get; }

    public EntityFrameworkEntityCrudGenerator(IDynamicClassGenerator classGenerator,
        IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> dbContextOptionsBuilder,
        Type dbContextType,
        bool usePooled) : base(classGenerator,
        services)
    {
        DbContextOptionsBuilder = dbContextOptionsBuilder;
        DbContextType = dbContextType;
        UsePooled = usePooled;
    }

    public EntityFrameworkEntityCrudGenerator AddEntity<TKey, TEntity>(
        Action<EntityFrameworkEntityBuilder<TKey, TEntity>> configure)
        where TKey : struct
        where TEntity : class, IEntity<TKey>, new()
    {
        var builder = new EntityFrameworkEntityBuilder<TKey, TEntity>(Services, DbContextType, DbContextOptionsBuilder, UsePooled);
        configure(builder);
        Builders.Add(builder);
        return this;
    }
}
