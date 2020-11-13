using System.Collections.Generic;
using System.Linq;

namespace Firebend.AutoCrud.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> source) => source == null || !source.Any();

        public static bool HasValues<T>(this IEnumerable<T> source) => source != null && source.Any();
    }
}
