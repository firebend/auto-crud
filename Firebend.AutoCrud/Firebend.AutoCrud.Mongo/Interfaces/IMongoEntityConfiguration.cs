namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoEntityConfiguration
    {
        public string CollectionName { get; set; }
        
        public string DatabaseName { get; set; }
    }
}