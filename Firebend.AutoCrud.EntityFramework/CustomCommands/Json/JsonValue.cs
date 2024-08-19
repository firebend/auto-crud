using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Firebend.AutoCrud.EntityFramework.CustomCommands.Json;

public static class JsonValue
{
    private static readonly MethodInfo Method
        = typeof(EfJsonFunctions)
            .GetMethod(nameof(EfJsonFunctions.JsonValue),
                [typeof(string), typeof(string)]);

    public static ModelBuilder AddJsonValueSupport(this ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(Method)
            .HasTranslation(args =>
            {
                if (args[0] is not SqlConstantExpression columnName)
                {
                    throw new ArgumentNullException(nameof(args));
                }

                if (string.IsNullOrEmpty(columnName.Value?.ToString()))
                {
                    throw new ArgumentNullException(nameof(columnName.Value));
                }

                var columnFragment = new SqlFragmentExpression(columnName.Value.ToString()!);

                var arrayValExpression = new SqlFunctionExpression(
                    "JSON_VALUE",
                    [columnFragment, args[1]],
                    true,
                    [false, false],
                    typeof(string),
                    null);
                return arrayValExpression;
            });

        return modelBuilder;
    }
}
