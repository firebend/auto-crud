#nullable enable
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Core.Implementations.Caching;

public class JsonEntityCacheSerializer : IEntityCacheSerializer
{
    public string? Serialize<T>(T value) where T : class
    {
        try
        {
            return JsonConvert.SerializeObject(value);
        }
        catch
        {
            return null;
        }
    }

    public T? Deserialize<T>(string value) where T : class
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
        catch
        {
            return null;
        }
    }
}
