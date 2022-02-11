using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.Io.Attributes;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class FileFieldAutoMapper<T> : IFileFieldAutoMapper<T>
        where T : class
    {
        private readonly IMemoizer<IFileFieldWrite<T>[]> _memoizer;
        private readonly IFileFieldWriteFilter<T> _filter;

        public FileFieldAutoMapper(IFileFieldWriteFilter<T> filter,
            IMemoizer<IFileFieldWrite<T>[]> memoizer)
        {
            _filter = filter;
            _memoizer = memoizer;
        }

        private IEnumerable<IFileFieldWrite<T>> MapOutputImpl()
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

                if (!_filter.ShouldExport(field))
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

        public IFileFieldWrite<T>[] MapOutput() =>
            _memoizer.Memoize($"AutoCrud.Io.Mapping.{typeof(T).FullName}", () => MapOutputImpl().OrderBy(x => x.FieldIndex).ToArray());
    }
}
