using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Io.Attributes;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;
using Firebend.JsonPatch.Extensions;

namespace Firebend.AutoCrud.Io.Implementations
{
    public class FileFieldAutoMapper<TVersion> : IFileFieldAutoMapper<TVersion>
        where TVersion : class, IAutoCrudApiVersion
    {
        private readonly IFileFieldWriteFilterFactory<TVersion> _filterFactory;

        public FileFieldAutoMapper(IFileFieldWriteFilterFactory<TVersion> filterFactory)
        {
            _filterFactory = filterFactory;
        }

        private IEnumerable<IFileFieldWrite<T>> MapOutputImpl<T>()
        {
            var properties = typeof(T).GetProperties();

            var hasAnyExportAttributes = properties.Any(x => x.GetCustomAttribute<ExportAttribute>() != null);

            var filter = _filterFactory.GetFilter<T>();
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

                if (!(filter?.ShouldExport(field) ?? true) || propertyInfo.PropertyType.IsCollection())
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

        public IFileFieldWrite<T>[] MapOutput<T>()
            where T : class => MapOutputImpl<T>().ToArray();
    }
}
