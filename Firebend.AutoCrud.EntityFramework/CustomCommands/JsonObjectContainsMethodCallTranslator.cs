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
    public class JsonObjectContainsMethodCallTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(FirebendAutoCrudDbFunctionExtensions).GetRuntimeMethod(
                nameof(FirebendAutoCrudDbFunctionExtensions.JsonContainsAny),
                new[] { typeof(DbFunctions), typeof(object), typeof(string) });

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public JsonObjectContainsMethodCallTranslator(ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public SqlExpression Translate(SqlExpression instance,
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

            var propertyReference = arguments[1];

            if (!(propertyReference is ColumnExpression columnExpression))
            {
                throw new InvalidOperationException("Invalid property");
            }var splatBuilder = AutoCrudObjectPool.StringBuilder.Get();
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

            return _sqlExpressionFactory.Function("functionName",
                functionArguments,
                true,
                functionArguments.Select(_ => false).ToList(),
                typeof(bool));
        }
    }
}
