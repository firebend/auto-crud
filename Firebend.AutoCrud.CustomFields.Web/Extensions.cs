using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.CustomFields.Web.Abstractions;
using Firebend.AutoCrud.Web;

namespace Firebend.AutoCrud.CustomFields.Web
{
    public static class Extensions
    {
        public static ControllerConfigurator<TBuilder, TKey, TEntity> WithCustomFieldsControllers<TBuilder, TKey, TEntity>(
            this ControllerConfigurator<TBuilder, TKey, TEntity> configurator)
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            var createController = typeof(AbstractCustomAttributeCreateController<,>)
                .MakeGenericType(configurator.Builder.EntityKeyType,
                    configurator.Builder.EntityType);

             configurator.WithController(createController, createController);

             var updateController = typeof(AbstractCustomAttributeUpdateController<,>)
                 .MakeGenericType(configurator.Builder.EntityKeyType,
                     configurator.Builder.EntityType);

             configurator.WithController(updateController, updateController);

             var deleteController = typeof(AbstractCustomAttributeDeleteController<,>)
                 .MakeGenericType(configurator.Builder.EntityKeyType,
                     configurator.Builder.EntityType);

             configurator.WithController(deleteController, deleteController);

             var searchController = typeof(AbstractCustomFieldsSearchController<,>)
                 .MakeGenericType(configurator.Builder.EntityKeyType,
                     configurator.Builder.EntityType);

             configurator.WithController(searchController, searchController);

             return configurator;
        }
    }
}
