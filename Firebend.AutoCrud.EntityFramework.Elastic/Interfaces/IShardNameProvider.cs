namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

public interface IShardNameProvider
{
    public string GetShardName(string key);
}
