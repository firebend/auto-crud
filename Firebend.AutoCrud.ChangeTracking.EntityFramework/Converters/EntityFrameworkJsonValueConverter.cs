using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.ChangeTracking.EntityFramework.Converters
{
    public class EntityFrameworkJsonValueConverter<T> : ValueConverter<T, string>
    {
        private static readonly Expression<Func<T, string>> ConvertTo = arg => JsonConvert.SerializeObject(arg);

        private static readonly Expression<Func<string, T>> ConvertFrom = arg => JsonConvert.DeserializeObject<T>(arg);

        public EntityFrameworkJsonValueConverter(ConverterMappingHints mappingHints = null) :
            base(ConvertTo, ConvertFrom, mappingHints)
        {
        }
    }
}
