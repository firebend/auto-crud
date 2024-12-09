namespace Firebend.AutoCrud.Caching.interfaces;

public interface IEntityCacheSerializer
{
    string Serialize<T>(T value);

    T Deserialize<T>(string value);
}
