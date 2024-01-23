using System;
using System.Linq;
using System.Linq.Expressions;
using Firebend.JsonPatch.Extensions;

namespace Firebend.AutoCrud.Core.Extensions;

public static class StringExtensions
{
    public static string Coalesce(this string source, string substitution)
        => string.IsNullOrWhiteSpace(source) ? substitution : source;

    public static T? ParseEnum<T>(this string source)
        where T : struct
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        if (Enum.TryParse(source, true, out T outEnum))
        {
            return outEnum;
        }

        return null;
    }

    public static bool In(this string source, params string[] list) => list.Any(x => x.EqualsIgnoreCaseAndWhitespace(source));

    public static string SafeTrim(this string source) => string.IsNullOrEmpty(source) ? source : source.Trim();

    public static string FirstCharToLower(this string str)
    {
        if (char.IsLower(str[0]))
        {
            return str;
        }

        return string.Create(str.Length, str, (output, input) =>
        {
            input.CopyTo(output);
            output[0] = char.ToLowerInvariant(input[0]);
        });
    }

    public static string TrimExtraPathSlashes(this string url) => url.Replace("//", "/");

    /// <summary>
    /// Takes a string property path and converts it to an expression selector.
    /// ex. given an object with a field "Name" pass "name" or "Name"
    /// ex. given an object with a nested field "Contact" with a field "FirstName" pass "contact.firstName" or "Contact.FirstName"
    /// </summary>
    /// <param name="name">
    /// The string property path
    /// </param>
    /// <typeparam name="T">
    /// The root objects type
    /// </typeparam>
    /// <returns>
    /// An expression
    /// </returns>
    public static Expression<Func<T, object>> ToPropertyExpressionSelector<T>(this string name)
    {
        var arg = Expression.Parameter(typeof(T), "x");
        Expression body = arg;

        foreach (var fieldName in name.Split('.'))
        {
            var member = fieldName;

            if (char.IsLower(fieldName[0]))
            {
                member = $"{char.ToUpper(fieldName[0])}{fieldName[1..]}";
            }

            try
            {
                body = Expression.Convert(Expression.PropertyOrField(body, member), typeof(object));
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        return Expression.Lambda<Func<T, object>>(body, arg);
    }
}
