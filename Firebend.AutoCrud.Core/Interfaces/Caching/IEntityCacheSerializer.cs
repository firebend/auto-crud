namespace Firebend.AutoCrud.Core.Interfaces.Caching;

public interface IEntityCacheSerializer
{
    public string Serialize<T>(T value);

    public T Deserialize<T>(string value);
}
