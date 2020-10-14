using Firebend.AutoCrud.Core.Abstractions;

namespace Firebend.AutoCrud.Web
{
    public static class ControllerEntityBuilderExtensions
    {
        public static ControllerEntityBuilder UsingControllers<TBuilder>(this TBuilder builder)
            where TBuilder : EntityCrudBuilder
        {
            return new ControllerEntityBuilder(builder);
        }
    }
}