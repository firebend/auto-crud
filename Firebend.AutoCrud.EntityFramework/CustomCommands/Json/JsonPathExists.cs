using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Firebend.AutoCrud.EntityFramework.CustomCommands.Json;

public static class JsonPathExists
{
    private static readonly MethodInfo Method
        = typeof(EfJsonFunctions)
            .GetMethod(nameof(EfJsonFunctions.JsonPathExists),
                [typeof(string), typeof(string)]);

    public static ModelBuilder AddJsonPathExistsSupport(this ModelBuilder modelBuilder)
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
                    "JSON_PATH_EXISTS",
                    [columnFragment, args[1]],
                    true,
                    [false, false],
                    typeof(int),
                    null);
                return arrayValExpression;
            });

        return modelBuilder;
    }
}
