using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firebend.AutoCrud.EntityFramework.Elastic.CustomCommands
{
    public class FullTextContainsAnyMethodCallTranslator : IMethodCallTranslator
    {
        private const string FreeTextFunctionName = "FREETEXT";
        private const string ContainsFunctionName = "CONTAINS";

        private static readonly MethodInfo _freeTextMethodInfo
            = typeof(FirebendAutoCrudDbFunctionExtensions).GetRuntimeMethod(
                nameof(FirebendAutoCrudDbFunctionExtensions.FreeTextAny),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        private static readonly MethodInfo _freeTextMethodInfoWithLanguage
            = typeof(FirebendAutoCrudDbFunctionExtensions).GetRuntimeMethod(
                nameof(FirebendAutoCrudDbFunctionExtensions.FreeTextAny),
                new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(int) });

        private static readonly MethodInfo _containsMethodInfo
            = typeof(FirebendAutoCrudDbFunctionExtensions).GetRuntimeMethod(
                nameof(FirebendAutoCrudDbFunctionExtensions.ContainsAny),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        private static readonly MethodInfo _containsMethodInfoWithLanguage
            = typeof(FirebendAutoCrudDbFunctionExtensions).GetRuntimeMethod(
                nameof(FirebendAutoCrudDbFunctionExtensions.ContainsAny),
                new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(int) });

        private static readonly IDictionary<MethodInfo, string> _typeMappings = new Dictionary<MethodInfo, string>
        {
            {_freeTextMethodInfo, FreeTextFunctionName},
            {_freeTextMethodInfoWithLanguage, FreeTextFunctionName},
            {_containsMethodInfo, ContainsFunctionName},
            {_containsMethodInfoWithLanguage, ContainsFunctionName}
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public FullTextContainsAnyMethodCallTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            if (_typeMappings == null)
            {
                return null;
            }

            if (!_typeMappings.TryGetValue(method, out var functionName))
            {
                return null;
            }

            var propertyReference = arguments[1];

            if (!(propertyReference is ColumnExpression))
            {
                throw new InvalidOperationException("Invalid property");
            }

            var splat = _sqlExpressionFactory.Fragment("*");
            var stringMap = new StringTypeMapping("nvarchar(max", DbType.String, true, null);
            var freeText = _sqlExpressionFactory.ApplyTypeMapping(arguments[2], stringMap);
            var functionArguments = new List<SqlExpression> { splat, freeText };

            if (arguments.Count == 4)
            {
                functionArguments.Add(
                    _sqlExpressionFactory.Fragment($"LANGUAGE {((SqlConstantExpression)arguments[3]).Value}"));
            }

            return _sqlExpressionFactory.Function(
                functionName,
                functionArguments,
                typeof(bool)
            );

        }
    }
}
