using System;
using System.Linq;
using System.Text;
namespace Firebend.AutoCrud.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool In(this string source, params string[] list)
        {
            return list.Any(x => x.EqualsIgnoreCaseAndWhitespace(source));
        }

        public static bool EqualsIgnoreCaseAndWhitespace(this string source, string compare)
        {
            if (source == null && compare == null)
            {
                return true;
            }

            if (source == null || compare == null)
            {
                return false;
            }

            return source.SafeTrim().Equals(compare.SafeTrim(), StringComparison.OrdinalIgnoreCase);
        }

        public static string SafeTrim(this string source)
        {
            return string.IsNullOrEmpty(source) ? source : source.Trim();
        }

        public static string ToKebabCase(this string source)
        {
            if (source is null) return null;

            if (source.Length == 0) return string.Empty;

            var builder = new StringBuilder();

            for (var i = 0; i < source.Length; i++)
            {
                if (char.IsWhiteSpace(source[i]))
                {
                    continue;
                }

                if (source[i] == '-')
                {
                    builder.Append(source[i]);
                }
                else if (char.IsLower(source[i])) // if current char is already lowercase
                {
                    builder.Append(source[i]);
                }
                else if (i == 0) // if current char is the first char
                {
                    builder.Append(char.ToLower(source[i]));
                }
                else if (char.IsLower(source[i - 1])) // if current char is upper and previous char is lower
                {
                    if (source[i - 1] != '-')
                    {
                        builder.Append("-");
                    }

                    builder.Append(char.ToLower(source[i]));
                }
                else if (i + 1 == source.Length || char.IsUpper(source[i + 1])) // if current char is upper and next char doesn't exist or is upper
                {
                    builder.Append(char.ToLower(source[i]));
                }
                else // if current char is upper and next char is lower
                {
                    if (i > 1 && source[i - 1] != '-')
                    {
                        builder.Append("-");
                    }

                    builder.Append(char.ToLower(source[i]));
                }
            }

            return builder.ToString();
        }

        public static string ToSentenceCase(this string source)
        {
            if (source is null) return null;

            if (source.Length == 0) return string.Empty;

            var builder = new StringBuilder();

            for (var i = 0; i < source.Length; i++)
            {
                if (char.IsLower(source[i])) // if current char is already lowercase
                {
                    builder.Append(source[i]);
                }
                else if (i == 0) // if current char is the first char
                {
                    builder.Append(source[i]);
                }
                else if (char.IsLower(source[i - 1])) // if current char is upper and previous char is lower
                {
                    builder.Append(" ");
                    builder.Append(source[i]);
                }
                else if (i + 1 == source.Length || char.IsUpper(source[i + 1])) // if current char is upper and next char doesn't exist or is upper
                {
                    builder.Append(source[i]);
                }
                else // if current char is upper and next char is lower
                {
                    builder.Append(" ");
                    builder.Append(source[i]);
                }
            }

            return builder.ToString();
        }
    }
}