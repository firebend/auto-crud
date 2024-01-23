namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

public interface IShardNameProvider
{
    string GetShardName(string key);
}
