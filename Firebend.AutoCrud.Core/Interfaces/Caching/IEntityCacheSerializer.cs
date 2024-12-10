#nullable enable
namespace Firebend.AutoCrud.Core.Interfaces.Caching;

public interface IEntityCacheSerializer
{
    /// <summary>
    /// Method to serialize an object to a string
    /// </summary>
    /// <param name="value">Object(T) to serialize</param>
    /// <typeparam name="T">Object representing a class</typeparam>
    /// <returns>A nullable string.</returns>
    public string? Serialize<T>(T value) where T : class;

    /// <summary>
    /// Method to deserialize a string to an object
    /// </summary>
    /// <param name="value">String to deserialize</param>
    /// <typeparam name="T">Object representing a class</typeparam>
    /// <returns>A nullable instance of T</returns>
    public T? Deserialize<T>(string value) where T : class;
}
