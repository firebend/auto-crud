using Firebend.AutoCrud.Core.Abstractions;

namespace Firebend.AutoCrud.Web
{
    public static class ControllerEntityBuilderExtensions
    {
        public static ControllerEntityBuilder<TBuilder> UsingControllers<TBuilder>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return new ControllerEntityBuilder<TBuilder>(builder);
        }
    }
}