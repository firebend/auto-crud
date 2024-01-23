using System;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web;

public static class ControllerEntityBuilderExtensions
{
    public static EntityCrudBuilder<TKey, TEntity> AddControllers<TKey, TEntity, TVersion>(this EntityCrudBuilder<TKey, TEntity> builder,
        Action<ControllerConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity, TVersion>> configure)
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
    {
        using var configurator = new ControllerConfigurator<EntityCrudBuilder<TKey, TEntity>, TKey, TEntity, TVersion>(builder);
        configure(configurator);
        return builder;
    }
}
