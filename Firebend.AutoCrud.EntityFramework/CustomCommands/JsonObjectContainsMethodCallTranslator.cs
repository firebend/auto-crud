using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Firebend.AutoCrud.Core.Pooling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Firebend.AutoCrud.EntityFramework.CustomCommands
{
    public class JsonObjectContainsMethodCallTranslator : IMethodCallTranslator
    {
        private const char LikeEscapeChar = '\\';
        private const string LikeEscapeString = "\\";

        private static readonly MethodInfo _methodInfo
            = typeof(FirebendAutoCrudDbFunctionExtensions).GetRuntimeMethod(
                nameof(FirebendAutoCrudDbFunctionExtensions.JsonContainsAny),
                new[] { typeof(DbFunctions), typeof(object), typeof(string) });

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public JsonObjectContainsMethodCallTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression? Translate(SqlExpression instance,
            MethodInfo method, IReadOnlyList<SqlExpression> arguments,
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

            if (method == _methodInfo && arguments.Count >= 3)
            {
                var pattern = arguments[2];
                var jsonObjectExpression = arguments[1];

                if (jsonObjectExpression is ColumnExpression columnExpression)
                {
                    var columnBuilder = AutoCrudObjectPool.StringBuilder.Get();
                    string columnString;

                    try
                    {

                        if (!string.IsNullOrWhiteSpace(columnExpression.Table?.Alias))
                        {
                            columnBuilder.Append('[');
                            columnBuilder.Append(columnExpression.Table.Alias);
                            columnBuilder.Append(']');
                            columnBuilder.Append('.');
                        }

                        columnBuilder.Append(columnExpression.Name);
                        columnString = columnBuilder.ToString();
                    }
                    finally
                    {
                        AutoCrudObjectPool.StringBuilder.Return(columnBuilder);
                    }

                    var columnFragment = _sqlExpressionFactory.Fragment(columnString);

                    var stringTypeMapping = ExpressionExtensions.InferTypeMapping(jsonObjectExpression);

                    if (pattern is SqlConstantExpression constantPattern)
                    {
                        if (!(constantPattern.Value is string patternValue))
                        {
                            return _sqlExpressionFactory.Like(
                                columnFragment,
                                _sqlExpressionFactory.Constant(null, stringTypeMapping));
                        }

                        if (patternValue.Length == 0)
                        {
                            return _sqlExpressionFactory.Constant(true);
                        }

                        return patternValue.Any(IsLikeWildChar)
                            ? _sqlExpressionFactory.Like(
                                columnFragment,
                                _sqlExpressionFactory.Constant($"%{EscapeLikePattern(patternValue)}%"),
                                _sqlExpressionFactory.Constant(LikeEscapeString))
                            : _sqlExpressionFactory.Like(columnFragment, _sqlExpressionFactory.Constant($"%{patternValue}%"));
                    }

                    if (pattern is SqlParameterExpression)
                    {
                        return _sqlExpressionFactory.Like(columnFragment, pattern, _sqlExpressionFactory.Constant(LikeEscapeString));
                    }
                }
            }

            return null;
        }

        private static bool IsLikeWildChar(char c)
            => c == '%' || c == '_' || c == '[';

        private string EscapeLikePattern(string pattern)
        {
            var builder = AutoCrudObjectPool.StringBuilder.Get();

            foreach (var c in pattern)
            {
                if (IsLikeWildChar(c)
                    || c == LikeEscapeChar)
                {
                    builder.Append(LikeEscapeChar);
                }

                builder.Append(c);
            }

            var ret = builder.ToString();
            AutoCrudObjectPool.StringBuilder.Return(builder);
            return ret;
        }
    }
}
