using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Firebend.AutoCrud.Core.Pooling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firebend.AutoCrud.EntityFramework.CustomCommands
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

            if (_typeMappings == null)
            {
                return null;
            }

            if (!_typeMappings.TryGetValue(method, out var functionName))
            {
                return null;
            }

            var propertyReference = arguments[1];

            if (!(propertyReference is ColumnExpression columnExpression))
            {
                throw new InvalidOperationException("Invalid property");
            }

            var splatBuilder = AutoCrudObjectPool.StringBuilder.Get();
            string splatString;

            try
            {

                if (!string.IsNullOrWhiteSpace(columnExpression.Table?.Alias))
                {
                    splatBuilder.Append('[');
                    splatBuilder.Append(columnExpression.Table.Alias);
                    splatBuilder.Append(']');
                    splatBuilder.Append('.');
                }

                splatBuilder.Append("*");
                splatString = splatBuilder.ToString();
            }
            finally
            {
                AutoCrudObjectPool.StringBuilder.Return(splatBuilder);
            }

            var splat = _sqlExpressionFactory.Fragment(splatString);
            var stringMap = new StringTypeMapping("nvarchar(max", DbType.String, true);
            var freeText = _sqlExpressionFactory.ApplyTypeMapping(arguments[2], stringMap);
            var functionArguments = new List<SqlExpression> { splat, freeText };

            if (arguments.Count == 4)
            {
                functionArguments.Add(
                    _sqlExpressionFactory.Fragment($"LANGUAGE {((SqlConstantExpression)arguments[3]).Value}"));
            }

            return _sqlExpressionFactory.Function(functionName,
                functionArguments,
                true,
                functionArguments.Select(_ => false).ToList(),
                typeof(bool));
        }
    }
}
