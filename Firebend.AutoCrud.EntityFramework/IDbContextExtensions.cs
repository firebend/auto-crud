using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Firebend.AutoCrud.EntityFramework
{
    public static class EfModelExtensions
    {
        public static string GetTableName<TEntity>(this IModel model)
            => GetTableName(model, typeof(TEntity));

        public static string GetTableName(this IModel model, Type entityType)
        {
            var efType = model.FindEntityType(entityType);

            if (efType != null)
            {
                return efType.GetTableName();
            }

            var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();
            var tableName = tableAttribute?.Name;

            if (string.IsNullOrWhiteSpace(tableName))
            {
                tableName = entityType.Name;
            }

            return tableName;
        }


        public static string GetSchemaName<TEntity>(this IModel model)
            => GetSchemaName(model, typeof(TEntity));

        public static string GetSchemaName(this IModel model, Type entityType)
        {
            var efType = model.FindEntityType(entityType);

            if (efType != null)
            {
                return efType.GetSchema();
            }

            var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();
            var schemaName = tableAttribute?.Schema;

            return string.IsNullOrWhiteSpace(schemaName) ? null : schemaName;
        }
    }
}
