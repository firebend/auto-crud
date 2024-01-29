using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Firebend.AutoCrud.EntityFramework.CustomCommands;

public class FullTextContainsAnyMethodCallTranslator : IMethodCallTranslator
{
    private const string FreeTextFunctionName = "FREETEXT";
    private const string ContainsFunctionName = "CONTAINS";

    private static readonly MethodInfo FreeTextMethodInfo
        = typeof(FirebendAutoCrudDbFunctionExtensions).GetRuntimeMethod(
            nameof(FirebendAutoCrudDbFunctionExtensions.FreeTextAny),
            [typeof(DbFunctions), typeof(string), typeof(string)]);

    private static readonly MethodInfo FreeTextMethodInfoWithLanguage
        = typeof(FirebendAutoCrudDbFunctionExtensions).GetRuntimeMethod(
            nameof(FirebendAutoCrudDbFunctionExtensions.FreeTextAny),
            [typeof(DbFunctions), typeof(string), typeof(string), typeof(int)]);

    private static readonly MethodInfo ContainsMethodInfo
        = typeof(FirebendAutoCrudDbFunctionExtensions).GetRuntimeMethod(
            nameof(FirebendAutoCrudDbFunctionExtensions.ContainsAny),
            [typeof(DbFunctions), typeof(string), typeof(string)]);

    private static readonly MethodInfo ContainsMethodInfoWithLanguage
        = typeof(FirebendAutoCrudDbFunctionExtensions).GetRuntimeMethod(
            nameof(FirebendAutoCrudDbFunctionExtensions.ContainsAny),
            [typeof(DbFunctions), typeof(string), typeof(string), typeof(int)]);

    private static readonly IDictionary<MethodInfo, string> TypeMappings = new Dictionary<MethodInfo, string>
    {
        {FreeTextMethodInfo, FreeTextFunctionName},
        {FreeTextMethodInfoWithLanguage, FreeTextFunctionName},
        {ContainsMethodInfo, ContainsFunctionName},
        {ContainsMethodInfoWithLanguage, ContainsFunctionName}
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

        if (TypeMappings == null)
        {
            return null;
        }

        if (!TypeMappings.TryGetValue(method, out var functionName))
        {
            return null;
        }

        var propertyReference = arguments[1];

        if (propertyReference is not ColumnExpression columnExpression)
        {
            throw new InvalidOperationException("Invalid property");
        }

        var splatString = !string.IsNullOrWhiteSpace(columnExpression.Table.Alias)
            ? $"[{columnExpression.Table.Alias}].*"
            : "*";

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
