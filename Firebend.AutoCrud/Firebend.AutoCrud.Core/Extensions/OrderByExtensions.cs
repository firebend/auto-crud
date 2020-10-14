using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Firebend.AutoCrud.Core.Extensions
{
    public static class OrderByExtensions
    {
        public static IEnumerable<(Expression<Func<T, object>> order, bool ascending)> ToOrderBy<T>(
            this Expression<Func<T, object>> source, bool ascending)
        {
            return source == null
                ? default(IEnumerable<(Expression<Func<T, object>> order, bool @ascending)>)
                : new List<(Expression<Func<T, object>> order, bool @ascending)> {(source, @ascending)};
        }

        public static IEnumerable<(Expression<Func<T, object>> order, bool ascending)> ToOrderByAscending<T>(
            this Expression<Func<T, object>> source)
        {
            return source.ToOrderBy(true);
        }

        public static IEnumerable<(Expression<Func<T, object>> order, bool ascending)> ToOrderByDescending<T>(
            this Expression<Func<T, object>> source)
        {
            return source.ToOrderBy(false);
        }

        public static IEnumerable<(Expression<Func<T, object>> order, bool ascending)> AddOrderByAscending<T>(
            this IEnumerable<(Expression<Func<T, object>> order, bool ascending)> source,
            Expression<Func<T, object>> orderBy)
        {
            if (orderBy == default) return source;

            var list = source?.ToList() ?? new List<(Expression<Func<T, object>> order, bool ascending)>();

            list.Add((orderBy, true));

            return list;
        }

        public static IEnumerable<(Expression<Func<T, object>> order, bool ascending)> AddOrderByDescending<T>(
            this IEnumerable<(Expression<Func<T, object>> order, bool ascending)> source,
            Expression<Func<T, object>> orderBy)
        {
            if (orderBy == default) return source;

            var list = source?.ToList() ?? new List<(Expression<Func<T, object>> order, bool ascending)>();

            list.Add((orderBy, false));

            return list;
        }

        public static IEnumerable<(Expression<Func<T, object>> order, bool ascending)> ToOrderByGroups<T>(
            this IEnumerable<string> source)
        {
            var orderFields = source?.ToList();

            if (orderFields?.Any() != true) return new List<(Expression<Func<T, object>> order, bool @ascending)>();

            return orderFields.Select(x => x.ToOrderByGroup<T>()).Where(x => x != default).ToList();
        }

        public static (Expression<Func<T, object>> order, bool ascending) ToOrderByGroup<T>(this string source)
        {
            if (source == null) return default;

            var spec = source.Split(':');

            var name = spec[0];

            if (string.IsNullOrWhiteSpace(name)) return default;

            if (char.IsLower(name[0])) name = $"{char.ToUpper(name[0])}{name.Substring(1)}";

            var type = typeof(T);

            var propertyInfo = type.GetProperty(name);

            if (propertyInfo == null) return default;

            var arg = Expression.Parameter(type, "x");
            Expression expr = null;

            expr = Expression.Property(arg, propertyInfo);
            if (propertyInfo.PropertyType.IsValueType)
                expr = Expression.Convert(expr, typeof(object));

            var expression = Expression.Lambda<Func<T, object>>(expr, arg);

            var descending = spec
                .Skip(1)
                .Take(1)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => char.ToLower(x.First()))
                .Select(x => x == 'd' || x == 'f')
                .FirstOrDefault();

            return (expression, !descending);
        }

        public static string GetMemberName(this LambdaExpression source)
        {
            if (source == null) return null;
            var body = source.Body;
            if (body is UnaryExpression unaryExpression) body = unaryExpression.Operand;

            if (body is MemberExpression memberExpression) return memberExpression.Member.Name;

            return null;
        }
    }
}