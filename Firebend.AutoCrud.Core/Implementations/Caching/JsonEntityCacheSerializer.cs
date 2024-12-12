#nullable enable
using System;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.Core.Implementations.Caching;

public class JsonEntityCacheSerializer(ILogger<JsonEntityCacheSerializer> logger) : IEntityCacheSerializer
{
    public string? Serialize<T>(T value) where T : class
    {
        try
        {
            return JsonConvert.SerializeObject(value);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to serialize object of type {Type}", typeof(T).Name);
            return null;
        }
    }

    public T? Deserialize<T>(string value) where T : class
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to deserialize object of type {Type}", typeof(T).Name);
            return null;
        }
    }
}
