using System;
using System.Linq;
using System.Reflection.Emit;
using Firebend.AutoCrud.Core.Implementations.Concurrency;

namespace Firebend.AutoCrud.Core.ObjectMapping
{
    /// <summary>
    /// This mapper class finds the matching properties and copies them from source object to target object. The copy function has IL codes to do this task.
    /// </summary>
    public class ObjectMapper : BaseObjectMapper
    {
        /// <summary>
        /// Easy access to mapper functions
        /// </summary>
        public static ObjectMapper Instance { get; } = new();

        private ObjectMapper()
        {
        }

        /// <summary>
        /// This function creates the mappings between objects and store the mappings in the private dictionary
        /// </summary>
        /// <param name="source">The type of the source object</param>
        /// <param name="target">The type of the target object</param>
        /// <param name="propertiesToIgnore">These string parameters will be ignored during matching process</param>
        /// <exception cref="InvalidOperationException">The Invalid Operation Exception will be thrown if it can't find the given property in source/target objects.</exception>
        protected override string MapTypes(Type source, Type target, params string[] propertiesToIgnore)
        {
            var key = GetMapKey(source, target, propertiesToIgnore);
            return key;
        }

        private DynamicMethod DynamicMethodFactory(string key, Type source, Type target, string[] propertiesToIgnore)
        {
            var args = new[] { source, target };

            var dm = new DynamicMethod(key, null, args);
            var il = dm.GetILGenerator();
            var maps = GetMatchingProperties(source, target);

            foreach (var map in maps)
            {
                if (propertiesToIgnore?.Contains(map.SourceProperty.Name) ?? false)
                {
                    continue;
                }

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Callvirt, map.SourceProperty.GetGetMethod() ?? throw new InvalidOperationException(), null);
                il.EmitCall(OpCodes.Callvirt, map.TargetProperty.GetSetMethod() ?? throw new InvalidOperationException(), null);
            }
            il.Emit(OpCodes.Ret);
            return dm;
        }

        /// <summary>
        /// This function copies all matched property values from source object to target object
        /// </summary>
        /// <param name="source">The original object that keeps the actual values/properties</param>
        /// <param name="target">The object that will get the related values from the given object</param>
        /// <param name="propertiesToIgnore">These string parameters will be ignored during matching process.
        /// It is best practice to have this as a static variable at all possible. Doing so will reduce
        /// memory allocations.
        /// </param>
        public override void Copy<TSource, TTarget>(TSource source, TTarget target, string[] propertiesToIgnore = null)
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            var key = MapTypes(sourceType, targetType, propertiesToIgnore);

            var dynamicMethod = Memoizer.Instance.Memoize<DynamicMethod, (string, Type, Type, string[], ObjectMapper)>(
                key, static factoryArg =>
            {
                var (dictKey, s, t, ignores, self) = factoryArg;

                var dynamicMethod = self.DynamicMethodFactory(dictKey, s, t, ignores);

                return dynamicMethod;
            }, (key, sourceType, targetType, propertiesToIgnore, this));

            dynamicMethod.Invoke(null, new object[] { source, target });
        }
    }
}
