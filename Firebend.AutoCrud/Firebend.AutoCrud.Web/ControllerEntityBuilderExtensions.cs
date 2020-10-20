using System;
using Firebend.AutoCrud.Core.Abstractions;
using Firebend.AutoCrud.Core.Abstractions.Builders;

namespace Firebend.AutoCrud.Web
{
    public static class ControllerEntityBuilderExtensions
    {
        public static TBuilder AddControllers<TBuilder>(this TBuilder builder, Action<ControllerConfigurator<TBuilder>> configure)
            where TBuilder : EntityCrudBuilder
        {
            configure(new ControllerConfigurator<TBuilder>(builder));
            return builder;
        }
    }
}