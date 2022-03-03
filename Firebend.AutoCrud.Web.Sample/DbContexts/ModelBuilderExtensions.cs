using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Firebend.AutoCrud.Web.Sample.DbContexts;

public static class ModelBuilderExtensions
{
    private static readonly MethodInfo JsonArrayIsEmptyMethod
        = typeof(DbFunctionsExtensions)
            .GetMethod(nameof(DbFunctionsExtensions.JsonArrayIsEmpty),
                new[] {typeof(string), typeof(string)});

    public static ModelBuilder AddJsonArrayIsEmptySupport(this ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(JsonArrayIsEmptyMethod)
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
                var arrayStringValExpression = new SqlFunctionExpression(
                    "JSON_QUERY",
                    new[] {columnFragment, args[1]},
                    true,
                    new[] {false, false},
                    typeof(string),
                    null);
                var emptyArrayConstantExpression = new SqlConstantExpression(Expression.Constant("[]"), null);
                var equalsExpression = new SqlBinaryExpression(ExpressionType.Equal, arrayStringValExpression,
                    emptyArrayConstantExpression, typeof(bool), null);
                return equalsExpression;
            });

        return modelBuilder;
    }

    private static readonly MethodInfo JsonArrayContainsMethod
        = typeof(DbFunctionsExtensions)
            .GetMethod(nameof(DbFunctionsExtensions.JsonValue),
                new[] {typeof(string), typeof(string)});

    public static ModelBuilder AddJsonValueSupport(this ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(JsonArrayContainsMethod)
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
                    "JSON_QUERY",
                    new[] {columnFragment, args[1]},
                    true,
                    new[] {false, false},
                    typeof(string),
                    null);
                return arrayValExpression;
            });

        return modelBuilder;
    }
}
