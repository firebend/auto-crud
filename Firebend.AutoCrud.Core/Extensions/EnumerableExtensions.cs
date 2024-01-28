using System.Collections.Generic;
using System.Linq;

namespace Firebend.AutoCrud.Core.Extensions;

public static class EnumerableExtensions
{
    public static bool IsEmpty<T>(this IEnumerable<T> source)
        => source switch
        {
            null => true,
            ICollection<T> collection => collection.Count == 0,
            _ => source.Any()
        };

    public static bool HasValues<T>(this IEnumerable<T> source) => source.IsEmpty() is false;

    public static IEnumerable<T> NullCheck<T>(this IEnumerable<T> source) => source ?? Enumerable.Empty<T>();
}
