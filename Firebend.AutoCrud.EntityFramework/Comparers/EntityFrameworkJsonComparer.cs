using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.EntityFramework.Comparers
{
    public class EntityFrameworkJsonComparer<T> : ValueComparer<T>
    {
        public EntityFrameworkJsonComparer(JsonSerializerSettings serializerSettings = null)
            : base((t1, t2) => DoEquals(t1, t2, serializerSettings),
                t => DoGetHashCode(t, serializerSettings),
                t => DoGetSnapshot(t, serializerSettings))
        {
        }

        private static string Json(T instance, JsonSerializerSettings settings) => JsonConvert.SerializeObject(instance, Formatting.None, settings);

        private static T DoGetSnapshot(T instance, JsonSerializerSettings settings)
        {
            if (instance is ICloneable cloneable)
            {
                return (T)cloneable.Clone();
            }

            return (T)JsonConvert.DeserializeObject(Json(instance, settings), typeof(T));
        }

        private static int DoGetHashCode(T instance, JsonSerializerSettings settings)
        {
            if (instance is IEquatable<T>)
            {
                return instance.GetHashCode();
            }

            return Json(instance, settings).GetHashCode();
        }

        private static bool DoEquals(T left, T right, JsonSerializerSettings settings)
        {
            if (left is IEquatable<T> equatable)
            {
                return equatable.Equals(right);
            }

            var result = Json(left, settings).Equals(Json(right, settings));

            return result;
        }
    }
}
