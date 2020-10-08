using System;

namespace Firebend.AutoCrud.Mongo.Models
{
    public class MongoDbMigrationVersion
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public DateTimeOffset Version { get; set; }
    }
}