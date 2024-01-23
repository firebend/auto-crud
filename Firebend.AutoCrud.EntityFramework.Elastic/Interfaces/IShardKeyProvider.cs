namespace Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;

public interface IShardKeyProvider
{
    string GetShardKey();
}
