using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Firebend.AutoCrud.EntityFramework
{
    public static class EfModelExtensions
    {
        private static string GetTableNameInternal(this IReadOnlyEntityType efType, MemberInfo entityType)
        {
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

        public static string GetTableName<TEntity>(this IModel model)
            => GetTableNameInternal(model.FindEntityType(typeof(TEntity)), typeof(TEntity));

        public static string GetTableName(this IMutableModel model, Type entityType)
            => GetTableNameInternal(model.FindEntityType(entityType), entityType);

        private static string GetSchemaNameInternal(this IReadOnlyEntityType efType, MemberInfo entityType)
        {
            if (efType != null)
            {
                return efType.GetSchema();
            }

            var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();
            var schemaName = tableAttribute?.Schema;

            return string.IsNullOrWhiteSpace(schemaName) ? null : schemaName;
        }

        public static string GetSchemaName<TEntity>(this IModel model)
            => GetSchemaNameInternal(model.FindEntityType(typeof(TEntity)), typeof(TEntity));

        public static string GetSchemaName(this IMutableModel model, Type entityType)
            => GetSchemaNameInternal(model.FindEntityType(entityType), entityType);
    }
}
