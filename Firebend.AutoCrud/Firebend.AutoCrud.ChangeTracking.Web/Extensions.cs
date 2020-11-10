using Firebend.AutoCrud.ChangeTracking.Web.Abstractions;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web;

namespace Firebend.AutoCrud.ChangeTracking.Web
{
    public static class Extensions
    {
        public static ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> WithChangeTrackingControllers<TBuilder, TKey, TEntity, TViewModel>(
            this ControllerConfigurator<TBuilder, TKey, TEntity, TViewModel> configurator)
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : class, IEntity<TKey>
            where TViewModel : class
            => configurator.WithController<AbstractChangeTrackingReadController<TKey, TEntity, TViewModel>>();
    }
}
