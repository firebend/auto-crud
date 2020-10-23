using Firebend.AutoCrud.ChangeTracking.Web.Abstractions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web;

namespace Firebend.AutoCrud.ChangeTracking.Web
{
    public static class Extensions
    {
        public static ControllerConfigurator<TBuilder, TKey, TEntity> WithChangeTrackingControllers<TBuilder, TKey, TEntity>(
            this ControllerConfigurator<TBuilder, TKey, TEntity> configurator)
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            var type = typeof(AbstractChangeTrackingReadController<,>);
            
            return configurator.WithController(type, type, typeof(TKey), typeof(TEntity));
        }
    }
}