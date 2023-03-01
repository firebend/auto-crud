using System;
using System.Reflection.Emit;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web;

public static class ControllerConfiguratorExtensions
{
    public static ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> AddResourceAuthorization<TBuilder, TKey,
        TEntity, TVersion>(
        this ControllerConfigurator<TBuilder, TKey, TEntity, TVersion> configurator, Type type, Type filterType,
        string policy)
        where TBuilder : EntityCrudBuilder<TKey, TEntity>
        where TKey : struct
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
    {
        var (attributeType, attributeBuilder) =
            GetResourceAuthorizationAttributeInfo(filterType, policy);
        configurator.Builder.WithAttribute(type, attributeType, attributeBuilder);
        return configurator;
    }

    private static (Type attributeType, CustomAttributeBuilder attributeBuilder)
        GetResourceAuthorizationAttributeInfo(
            Type filterType,
            string policy)
    {
        var authCtor = filterType.GetConstructor(new[] { typeof(string) });

        if (authCtor == null)
        {
            return default;
        }

        var args = new object[] { policy };
        return (filterType,
            new CustomAttributeBuilder(authCtor, args));
    }
}
