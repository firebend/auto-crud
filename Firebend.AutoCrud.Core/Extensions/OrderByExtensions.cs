using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Firebend.AutoCrud.Core.Extensions;

public static class OrderByExtensions
{
    public static IEnumerable<(Expression<Func<T, object>> order, bool ascending)> ToOrderBy<T>(
        this Expression<Func<T, object>> source,
        bool ascending) => source == null
        ? default(IEnumerable<(Expression<Func<T, object>> order, bool ascending)>)
        : new List<(Expression<Func<T, object>> order, bool ascending)> { (source, ascending) };

    public static IEnumerable<(Expression<Func<T, object>> order, bool ascending)> ToOrderByAscending<T>(
        this Expression<Func<T, object>> source) => source.ToOrderBy(true);

    public static IEnumerable<(Expression<Func<T, object>> order, bool ascending)> ToOrderByDescending<T>(
        this Expression<Func<T, object>> source) => source.ToOrderBy(false);

    public static IEnumerable<(Expression<Func<T, object>> order, bool ascending)> AddOrderByAscending<T>(
        this IEnumerable<(Expression<Func<T, object>> order, bool ascending)> source,
        Expression<Func<T, object>> orderBy)
    {
        if (orderBy == default)
        {
            return source;
        }

        var list = source?.ToList() ?? [];

        list.Add((orderBy, true));

        return list;
    }

    public static IEnumerable<(Expression<Func<T, object>> order, bool ascending)> AddOrderByDescending<T>(
        this IEnumerable<(Expression<Func<T, object>> order, bool ascending)> source,
        Expression<Func<T, object>> orderBy)
    {
        if (orderBy == default)
        {
            return source;
        }

        var list = source?.ToList() ?? [];

        list.Add((orderBy, false));

        return list;
    }

    public static (Expression<Func<T, object>> order, bool ascending)[] ToOrderByGroups<T>(
        this string[] source)
    {
        if (source is null || source.Length <= 0)
        {
            return Array.Empty<(Expression<Func<T, object>> order, bool ascending)>();
        }

        return source
            .Select(x => x.ToOrderByGroup<T>())
            .Where(x => x != default)
            .ToArray();
    }

    public static (Expression<Func<T, object>> order, bool ascending) ToOrderByGroup<T>(this string source)
    {
        if (source == null)
        {
            return default;
        }

        var spec = source.Split(':');

        var name = spec[0];

        if (string.IsNullOrWhiteSpace(name))
        {
            return default;
        }

        var expression = name.ToPropertyExpressionSelector<T>();

        if (expression is null)
        {
            return default;
        }

        var descending = spec
            .Skip(1)
            .Take(1)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => char.ToLower(x.First()))
            .Select(x => x is 'd')
            .FirstOrDefault();

        return (expression, descending is false);
    }
}
