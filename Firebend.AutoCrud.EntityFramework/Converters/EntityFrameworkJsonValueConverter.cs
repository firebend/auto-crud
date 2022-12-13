using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.EntityFramework.Converters
{
    public class EntityFrameworkJsonValueConverter<T> : ValueConverter<T, string>
    {
        public EntityFrameworkJsonValueConverter(ConverterMappingHints mappingHints = null) :
            base(arg => JsonConvert.SerializeObject(arg),
                arg => JsonConvert.DeserializeObject<T>(arg), mappingHints)
        {
        }
    }
}
