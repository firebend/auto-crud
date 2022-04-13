using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Firebend.AutoCrud.Io.Attributes;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;
using Firebend.JsonPatch.Extensions;

namespace Firebend.AutoCrud.Io.Implementations
{
    public static class FileFieldAutoMapperCaches<T>
    {
        public static readonly ConcurrentDictionary<string, IFileFieldWrite<T>[]> Caches = new();
    }
    public class FileFieldAutoMapper : IFileFieldAutoMapper<T>
        where T : class
    {
        private readonly IFileFieldWriteFilter<T> _filter;

        public FileFieldAutoMapper(IFileFieldWriteFilter<T> filter)
        {
            _filter = filter;
        }

        private IEnumerable<IFileFieldWrite<T>> MapOutputImpl<T>()
        {
            var properties = typeof(T).GetProperties();

            var hasAnyExportAttributes = properties.Any(x => x.GetCustomAttribute<ExportAttribute>() != null);

            var index = 0;

            foreach (var propertyInfo in properties)
            {
                var ctExport = propertyInfo.GetCustomAttribute<ExportAttribute>();

                if (ctExport == null && hasAnyExportAttributes)
                {
                    continue;
                }

                var field = new FileFieldWrite<T>
                {
                    FieldIndex = ctExport?.Order ?? index++,
                    FieldName = ctExport?.Name
                                ?? propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
                                ?? propertyInfo.Name
                };

                if (!_filter.ShouldExport(field) || propertyInfo.PropertyType.IsCollection())
                {
                    continue;
                }

                var parameter = Expression.Parameter(typeof(T), "x");
                var property = Expression.Property(parameter, propertyInfo);
                var conversion = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda<Func<T, object>>(conversion, parameter);

                field.Writer = lambda.Compile();

                yield return field;
            }
        }

        public IFileFieldWrite<T>[] MapOutput<T>() => FileFieldAutoMapperCaches<T>
            .Caches
            .GetOrAdd(typeof(T).FullName, static (_, arg) =>
                arg.MapOutputImpl<T>().OrderBy(x => x.FieldIndex).ToArray(), this);
    }

    public static class FileFieldAutoMapper
    {
        private static IEnumerable<IFileFieldWrite<T>> MapOutputImpl<T>()
            where T : class
        {
            var properties = typeof(T).GetProperties();

            var hasAnyExportAttributes = properties.Any(x => x.GetCustomAttribute<ExportAttribute>() != null);

            var index = 0;

            foreach (var propertyInfo in properties)
            {
                var ctExport = propertyInfo.GetCustomAttribute<ExportAttribute>();

                if (ctExport == null && hasAnyExportAttributes)
                {
                    continue;
                }

                var field = new FileFieldWrite<T>
                {
                    FieldIndex = ctExport?.Order ?? index++,
                    FieldName = ctExport?.Name
                                ?? propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
                                ?? propertyInfo.Name
                };

                if (propertyInfo.PropertyType.IsCollection()) //
                {
                    continue;
                }

                var parameter = Expression.Parameter(typeof(T), "x");
                var property = Expression.Property(parameter, propertyInfo);
                var conversion = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda<Func<T, object>>(conversion, parameter);

                field.Writer = lambda.Compile();

                yield return field;
            }
        }

        public static IFileFieldWrite<T>[] MapOutput<T>()
            where T : class
        {
            return FileFieldAutoMapperCaches<T>
                .Caches
                .GetOrAdd(typeof(T).FullName,
                    static (_, arg) => MapOutputImpl<T>().OrderBy(x => x.FieldIndex).ToArray(), "");
        }
    }
}
