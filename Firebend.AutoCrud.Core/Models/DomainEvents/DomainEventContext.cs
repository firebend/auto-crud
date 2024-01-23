using Newtonsoft.Json;

namespace Firebend.AutoCrud.Core.Models.DomainEvents;

/// <summary>
///     Encapsulates data about a entity change event.
/// </summary>
public class DomainEventContext
{
    public string CustomContextJson { get; private set; }

    /// <summary>
    ///     The user who affected the entity.
    /// </summary>
    public string UserEmail { get; set; }

    /// <summary>
    ///     The source where the change originated from.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     An object where a custom context can be provided to enrich the domain event.
    /// </summary>
    [JsonIgnore]
    public object CustomContext
    {
        get => CustomContextJson == null
            ? null
            : JsonConvert.DeserializeObject(CustomContextJson, GetJsonSettings());

        set => CustomContextJson = value == null
            ? null
            : JsonConvert.SerializeObject(value, GetJsonSettings());
    }

    /// <summary>
    ///     Gets <see cref="CustomContext" /> typed to a specific instance.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of your custom context.
    /// </typeparam>
    /// <returns>
    ///     The Custom Context
    /// </returns>
    public T GetCustomContext<T>()
    {
        var obj = CustomContext;

        if (obj is T context)
        {
            return context;
        }

        if (string.IsNullOrWhiteSpace(CustomContextJson))
        {
            return default;
        }

        var deserialized = JsonConvert.DeserializeObject<T>(CustomContextJson, GetJsonSettings());

        return deserialized;
    }

    private static JsonSerializerSettings GetJsonSettings() => new() { TypeNameHandling = TypeNameHandling.All };
}
