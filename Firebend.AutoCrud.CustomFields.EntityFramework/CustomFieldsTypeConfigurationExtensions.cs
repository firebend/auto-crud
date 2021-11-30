using System;
using System.Linq;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.CustomFields.EntityFramework.Models;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.CustomFields.EntityFramework
{
    public static class CustomFieldsTypeConfigurationExtensions
    {
        public static void AddCustomFieldsConfigurations(this IDbContext context, ModelBuilder builder)
        {

            var entityTypes = context
                .GetType()
                .GetProperties()
                .Where(x => x.PropertyType.IsAssignableToGenericType(typeof(DbSet<>)))
                .Where(x =>
                {
                    var args = x.PropertyType.GetGenericArguments().First();
                    var isCustomFieldEntity = args.IsAssignableToGenericType(typeof(ICustomFieldsEntity<>)) &&
                                              args.IsAssignableToGenericType(typeof(IEntity<>));
                    return isCustomFieldEntity;
                })
                .Select(x => x.PropertyType.GetGenericArguments().First())
                .ToList();

            var entityMethod = builder
                .GetType()
                .GetMethods()
                .FirstOrDefault(x => x.Name == nameof(ModelBuilder.Entity) && x.IsGenericMethod);

            foreach (var entityType in entityTypes)
            {
                var entityKey = entityType.GetInterfaces()
                    .Where(x => x.IsGenericType)
                    .FirstOrDefault(x => x.IsAssignableToGenericType(typeof(IEntity<>)))
                    ?.GetGenericArguments()
                    .FirstOrDefault();

                if (entityKey == null)
                {
                    continue;
                }

                var isTenantEntity = entityType.IsAssignableToGenericType(typeof(ITenantEntity<>));
                Type tenantKeyType = null;

                if (isTenantEntity)
                {
                    tenantKeyType = entityType.GetInterfaces()
                        .Where(x => x.IsGenericType)
                        .FirstOrDefault(x => x.IsAssignableToGenericType(typeof(ITenantEntity<>)))
                        ?.GetGenericArguments()
                        .FirstOrDefault();

                    if (tenantKeyType == null)
                    {
                        throw new Exception("Could not determine tenant key type for entity " + entityType.Name);
                    }
                }

                var customFieldsEntityType = isTenantEntity ?
                    typeof(EfCustomFieldsModelTenant<,,>).MakeGenericType(entityKey, entityType, tenantKeyType) :
                    typeof(EfCustomFieldsModel<,>).MakeGenericType(entityKey, entityType);

                var configType = isTenantEntity ? typeof(CustomFieldEntityTenantTypeConfiguration<,,>).MakeGenericType(entityKey, entityType, tenantKeyType) :
                    typeof(CustomFieldEntityTypeConfiguration<,>).MakeGenericType(entityKey, entityType);

                var tableName = $"{builder.Model.GetTableName(entityType)}_CustomFields";
                var schemaName = builder.Model.GetSchemaName(entityType);

                var instance = Activator.CreateInstance(configType, tableName, schemaName);

                var configureMethod = configType.GetMethod("Configure");

                var entityMethodGeneric = entityMethod.MakeGenericMethod(customFieldsEntityType);

                var entityResult = entityMethodGeneric.Invoke(builder, new object[]
                {
                });

                configureMethod.Invoke(instance, new[]
                {
                    entityResult
                });
            }
        }
    }
}
