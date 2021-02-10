using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.CustomFields.Web.Abstractions;
using Firebend.AutoCrud.Web;

namespace Firebend.AutoCrud.CustomFields.Web
{
    public static class Extensions
    {
        public static ControllerConfigurator<TBuilder, TKey, TEntity> WithCustomFieldsControllers<TBuilder, TKey, TEntity>(
            this ControllerConfigurator<TBuilder, TKey, TEntity> configurator,
            string entityName = null,
            string entityNamePlural = null,
            string openApiName = null)
            where TBuilder : EntityCrudBuilder<TKey, TEntity>
            where TKey : struct
            where TEntity : class, IEntity<TKey>
        {
            var createController = typeof(AbstractCustomFieldsCreateController<,>)
                .MakeGenericType(configurator.Builder.EntityKeyType,
                    configurator.Builder.EntityType);

            configurator.WithController(createController, createController, entityName, entityNamePlural, openApiName);

            var updateController = typeof(AbstractCustomAttributeUpdateController<,>)
                .MakeGenericType(configurator.Builder.EntityKeyType,
                    configurator.Builder.EntityType);

            configurator.WithController(updateController, updateController, entityName, entityNamePlural, openApiName);

            var deleteController = typeof(AbstractCustomFieldsDeleteController<,>)
                .MakeGenericType(configurator.Builder.EntityKeyType,
                    configurator.Builder.EntityType);

            configurator.WithController(deleteController, deleteController, entityName, entityNamePlural, openApiName);

            var searchController = typeof(AbstractCustomFieldsSearchController<,>)
                .MakeGenericType(configurator.Builder.EntityKeyType,
                    configurator.Builder.EntityType);

            configurator.WithController(searchController, searchController, entityName, entityNamePlural, openApiName);

            return configurator;
        }
    }
}
