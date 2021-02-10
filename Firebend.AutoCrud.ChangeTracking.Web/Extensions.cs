using Firebend.AutoCrud.ChangeTracking.Web.Abstractions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web;

namespace Firebend.AutoCrud.ChangeTracking.Web
{
    public static class Extensions
    {
        public static ControllerConfigurator<TBuilder, TKey, TEntity> WithChangeTrackingControllers<TBuilder, TKey, TEntity>(
            this ControllerConfigurator<TBuilder, TKey, TEntity> configurator,
            string entityName = null,
            string entityNamePlural = null,
            string openApiName = null)
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            var controller = typeof(AbstractChangeTrackingReadController<,,>)
                .MakeGenericType(configurator.Builder.EntityKeyType,
                    configurator.Builder.EntityType,
                    configurator.ReadViewModelType);

            return configurator.WithController(controller, controller, entityName, entityNamePlural, openApiName);
        }
    }
}
