namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoShardKeyProvider
{
    public string GetShardKey();
}
