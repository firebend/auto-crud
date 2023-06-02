using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Firebend.AutoCrud.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
            {
                return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            var baseType = givenType.BaseType;

            return baseType != null && IsAssignableToGenericType(baseType, genericType);
        }

        public static bool IsCollection(this Type type) =>
            type is not null && !ReferenceEquals(type, typeof(string)) && typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type);
    }
}
