using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Firebend.AutoCrud.Web.Sample.Extensions;

public static class HelperExtensions
{
    public static void SetPropertyValue<T, TValue>(this T target, Expression<Func<T, TValue>> memberLamda, TValue value)
    {
        if (memberLamda.Body is MemberExpression memberSelectorExpression)
        {
            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property != null)
            {
                property.SetValue(target, value, null);
            }
        }
    }

    public static bool PropertyEquals<T, TValue>(this T target, Expression<Func<T, TValue>> memberLamda,
        TValue expectedValue)
    {
        if (memberLamda.Body is MemberExpression memberSelectorExpression)
        {
            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property != null)
            {
                return property.GetValue(target) is TValue val && val.Equals(expectedValue);
            }
        }

        return false;
    }

    public static T ThrowExceptionIfNull<T>(this T possibleNull, string message)
    {
        if (possibleNull is null)
        {
            throw new Exception(message);
        }

        return possibleNull;
    }
}
