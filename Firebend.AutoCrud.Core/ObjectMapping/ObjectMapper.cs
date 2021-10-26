using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

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
        public static ObjectMapper Instance { get; protected set; } = new ObjectMapper();

        private ObjectMapper() { }

        /// <summary>
        /// This dictionary keeps the convert functions for objects by auto generated names
        /// </summary>
        private readonly Dictionary<string, DynamicMethod> _del = new Dictionary<string, DynamicMethod>();

        /// <summary>
        /// This function creates the mappings between objects and store the mappings in the private dictionary
        /// </summary>
        /// <param name="source">The type of the source object</param>
        /// <param name="target">The type of the target object</param>
        /// <param name="propertiesToIgnore">These string parameters will be ignored during matching process</param>
        /// <exception cref="InvalidOperationException">The Invalid Operation Exception will be thrown if it can't find the given property in source/target objects.</exception>
        protected override string MapTypes(Type source, Type target, params string[] propertiesToIgnore)
        {
            var key = GetMapKey(source, target);

            if (propertiesToIgnore.Length > 0)
            {
                key = $"temp_{key}";
            }

            if (_del.ContainsKey(key))
            {
                return key;
            }

            var args = new[] { source, target };

            var dm = new DynamicMethod(key, null, args);
            var il = dm.GetILGenerator();
            var maps = GetMatchingProperties(source, target);

            foreach (var map in maps)
            {
                if (propertiesToIgnore.Contains(map.SourceProperty.Name))
                {
                    continue;
                }

                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldarg_0);
                il.EmitCall(OpCodes.Callvirt, map.SourceProperty.GetGetMethod() ?? throw new InvalidOperationException(), null);
                il.EmitCall(OpCodes.Callvirt, map.TargetProperty.GetSetMethod() ?? throw new InvalidOperationException(), null);
            }
            il.Emit(OpCodes.Ret);
            _del.Add(key, dm);

            return key;
        }

        /// <summary>
        /// This function copies all matched property values from source object to target object
        /// </summary>
        /// <param name="source">The original object that keeps the actual values/properties</param>
        /// <param name="target">The object that will get the related values from the given object</param>
        /// <param name="propertiesToIgnore">These string parameters will be ignored during matching process</param>
        public override void Copy(object source, object target, params string[] propertiesToIgnore)
        {
            var sourceType = source.GetType();
            var targetType = target.GetType();

            var key = this.MapTypes(sourceType, targetType, propertiesToIgnore);

            var del = _del[key];
            var args = new[] { source, target };
            del.Invoke(null, args);

            if (propertiesToIgnore.Length > 0)
            {
                _del.Remove(key);
            }
        }
    }
}
