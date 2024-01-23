using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.EntityFramework.Converters;

public class EntityFrameworkJsonValueConverter<T> : ValueConverter<T, string>
{
    public EntityFrameworkJsonValueConverter(JsonSerializerSettings serializerSettings = null, ConverterMappingHints mappingHints = null) :
        base(arg => JsonConvert.SerializeObject(arg, serializerSettings),
            arg => JsonConvert.DeserializeObject<T>(arg, serializerSettings), mappingHints)
    {
    }
}
