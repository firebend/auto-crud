using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Comparers
{
    public class EntityFrameworkJsonComparer<T> : ValueComparer<T>
    {
        public EntityFrameworkJsonComparer()
            : base((t1, t2) => DoEquals(t1, t2), 
                t => DoGetHashCode(t), 
                t => DoGetSnapshot(t))
        {
        }

        private static string Json(T instance)
        {
            return JsonConvert.SerializeObject(instance, Formatting.None);
        }

        private static T DoGetSnapshot(T instance)
        {
            if (instance is ICloneable cloneable)
            {
                return (T) cloneable.Clone();
            }

            return (T)JsonConvert.DeserializeObject(Json(instance), typeof(T));
        }

        private static int DoGetHashCode(T instance)
        {
            if (instance is IEquatable<T>)
            {
                return instance.GetHashCode();
            }

            return Json(instance).GetHashCode();
        }

        private static bool DoEquals(T left, T right)
        {
            if (left is IEquatable<T> equatable)
            {
                return equatable.Equals(right);
            }

            var result = Json(left).Equals(Json(right));
            
            return result;
        }
    }
}