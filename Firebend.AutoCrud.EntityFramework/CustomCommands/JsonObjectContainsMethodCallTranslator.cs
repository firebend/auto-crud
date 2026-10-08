using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Firebend.AutoCrud.EntityFramework.CustomCommands;

public class JsonObjectContainsMethodCallTranslator : IMethodCallTranslator
{
    private const char LikeEscapeChar = '\\';
    private const string LikeEscapeString = "\\";

    private static readonly MethodInfo MethodInfo
        = typeof(FirebendAutoCrudDbFunctionExtensions).GetRuntimeMethod(
            nameof(FirebendAutoCrudDbFunctionExtensions.JsonContainsAny),
            [
                typeof(DbFunctions),
                typeof(object),
                typeof(string)
            ]);

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public JsonObjectContainsMethodCallTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    public SqlExpression Translate(SqlExpression instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method == null)
        {
            throw new ArgumentNullException(nameof(method));
        }

        if (arguments == null)
        {
            throw new ArgumentNullException(nameof(arguments));
        }

        if (method != MethodInfo || arguments.Count < 3)
        {
            return null;
        }

        var pattern = arguments[2];
        var jsonObjectExpression = arguments[1];

        if (jsonObjectExpression is not ColumnExpression columnExpression)
        {
            return null;
        }

        var columnString = !string.IsNullOrWhiteSpace(columnExpression.TableAlias)
            ? $"[{columnExpression.TableAlias}].{columnExpression.Name}"
            : columnExpression.Name;

        var columnFragment = _sqlExpressionFactory.Fragment(columnString);

        // The column fragment carries no type mapping, so LIKE cannot infer one for the pattern; EF Core 10 then
        // refuses to generate SQL ("does not have a type mapping assigned"). Give every pattern the provider's plain
        // string mapping: the column's own mapping can carry a value converter (e.g. object <-> JSON) that would try
        // to convert the pattern itself.
        var stringTypeMapping = _sqlExpressionFactory
            .ApplyDefaultTypeMapping(_sqlExpressionFactory.Constant(string.Empty))
            .TypeMapping;

        SqlExpression StringConstant(string value) => _sqlExpressionFactory.Constant(value, stringTypeMapping);

        switch (pattern)
        {
            case SqlConstantExpression constantPattern:
            {
                if (constantPattern.Value is not string patternValue)
                {
                    return _sqlExpressionFactory.Like(columnFragment, StringConstant(null!));
                }

                if (patternValue.Length == 0)
                {
                    return _sqlExpressionFactory.Constant(true);
                }

                return patternValue.Any(IsLikeWildChar)
                    ? _sqlExpressionFactory.Like(
                        columnFragment,
                        StringConstant($"%{EscapeLikePattern(patternValue)}%"),
                        StringConstant(LikeEscapeString))
                    : _sqlExpressionFactory.Like(columnFragment, StringConstant($"%{patternValue}%"));
            }
            case SqlParameterExpression:
                return _sqlExpressionFactory.Like(
                    columnFragment,
                    _sqlExpressionFactory.ApplyTypeMapping(pattern, stringTypeMapping),
                    StringConstant(LikeEscapeString));
            default:
                return null;
        }
    }

    private static bool IsLikeWildChar(char c) => c is '%' or '_' or '[';

    private static string EscapeLikePattern(string pattern)
    {
        var builder = new StringBuilder();

        foreach (var c in pattern)
        {
            if (IsLikeWildChar(c) || c == LikeEscapeChar)
            {
                builder.Append(LikeEscapeChar);
            }

            builder.Append(c);
        }

        var ret = builder.ToString();
        builder.Clear();

        return ret;
    }
}
