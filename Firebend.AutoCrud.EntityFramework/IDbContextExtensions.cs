using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.EntityFramework
{
    public static class IDbContextExtensions
    {
        public static string GetTableName<TEntity>(this IDbContext context)
        {
            var entityType = typeof(TEntity);

            var efType = context.Model.FindEntityType(entityType);

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


        public static string GetSchemaName<TEntity>(this IDbContext context)
        {
            var entityType = typeof(TEntity);

            var efType = context.Model.FindEntityType(entityType);

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
