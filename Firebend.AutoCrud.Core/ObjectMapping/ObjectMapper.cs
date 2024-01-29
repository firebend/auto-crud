using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Firebend.AutoCrud.Core.Extensions;

namespace Firebend.AutoCrud.Core.ObjectMapping;

public record ObjectMapperContext(
    Type SourceType,
    Type TargetType,
    string[] PropertiesToIgnore,
    string[] PropertiesToInclude,
    bool IncludeObjects)
{
    public string GetMapKey()
    {
        List<string> keys = ["ObjectMapper", SourceType.FullName, TargetType.FullName];

        if (!PropertiesToIgnore.IsEmpty())
        {
            keys.Add("propertiesToIgnore");
            keys.AddRange(PropertiesToIgnore);
        }

        if (!PropertiesToInclude.IsEmpty())
        {
            keys.Add("propertiesToInclude");
            keys.AddRange(PropertiesToInclude);
        }

        keys.Add($"_includeObjects_{IncludeObjects}");

        return string.Join('_', keys);
    }
}

/// <summary>
/// This mapper class finds the matching properties and copies them from source object to target object. The copy function has IL codes to do this task.
/// </summary>
public static class ObjectMapper
{
    private static readonly ConcurrentDictionary<ObjectMapperContext, DynamicMethod> Caches = new();

    public static IEnumerable<PropertyMap> GetMatchingProperties(ObjectMapperContext context)
    {
        var sourceProperties = context.SourceType.GetProperties()
            .Where(x => context.PropertiesToIgnore?.Contains(x.Name) is not false
                        && (context.PropertiesToInclude is null || context.PropertiesToInclude.Contains(x.Name)));

        var targetProperties = context.TargetType.GetProperties();

        var properties = sourceProperties.Join(
                targetProperties,
                x => x.Name,
                x => x.Name,
                (source, target) => new PropertyMap(source, target))
            .Where(x => x.SourceProperty.CanRead)
            .Where(x => x.TargetProperty.CanWrite)
            .Where(x => context.IncludeObjects
                        || x.SourceProperty.PropertyType.IsValueType
                        || x.SourceProperty.PropertyType == typeof(string))
            .Where(x => (x.SourceProperty.PropertyType.IsValueType
                         && x.SourceProperty.PropertyType.IsAssignableTo(x.TargetProperty.PropertyType))
                        || x.SourceProperty.PropertyType == x.TargetProperty.PropertyType);

        return properties;
    }

    private static DynamicMethod DynamicMethodFactory(ObjectMapperContext context)
    {
        var dm = new DynamicMethod(context.GetMapKey(),
            null,
            [context.SourceType, context.TargetType],
            false);

        var il = dm.GetILGenerator();
        var maps = GetMatchingProperties(context);

        foreach (var map in maps)
        {
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(OpCodes.Callvirt, map.SourceProperty.GetGetMethod() ?? throw new InvalidOperationException(), null);

            var targetUnderlyingType = Nullable.GetUnderlyingType(map.TargetProperty.PropertyType);
            var targetIsNullable = targetUnderlyingType != null;

            if (targetIsNullable && targetUnderlyingType == map.SourceProperty.PropertyType)
            {
                var constructor = typeof(Nullable<>).MakeGenericType(targetUnderlyingType)
                    .GetConstructor([targetUnderlyingType]) ?? throw new Exception();

                il.Emit(OpCodes.Newobj, constructor);
            }

            il.EmitCall(OpCodes.Callvirt, map.TargetProperty.GetSetMethod() ?? throw new InvalidOperationException(), null);
        }
        il.Emit(OpCodes.Ret);

        return dm;
    }

    public static void Copy<TSource, TTarget>(
        TSource source,
        TTarget target,
        string[] propertiesToIgnore = null,
        string[] propertiesToInclude = null,
        bool includeObjects = true,
        bool useMemoizer = true)
    {
        var context = new ObjectMapperContext(source.GetType(), target.GetType(), propertiesToIgnore, propertiesToInclude, includeObjects);

        if (useMemoizer is false)
        {
            var dm = DynamicMethodFactory(context);
            var action = dm.CreateDelegate<Action<TSource, TTarget>>();
            action(source, target);
            return;
        }

        var cache = Caches.GetOrAdd(context, DynamicMethodFactory);
        var act = cache.CreateDelegate<Action<TSource, TTarget>>();
        act(source, target);
    }
}
