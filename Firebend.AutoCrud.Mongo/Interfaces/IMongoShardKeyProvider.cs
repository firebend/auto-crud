namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoShardKeyProvider
    {
        string GetShardKey();
    }
}
