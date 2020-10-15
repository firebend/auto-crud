#region

using System;
using MongoDB.Bson;

#endregion

namespace Firebend.AutoCrud.Mongo.Configuration
{
    public class MongoDateTimeOffsetBsonTypeMapper : ICustomBsonTypeMapper
    {
        public bool TryMapToBsonValue(object value, out BsonValue bsonValue)
        {
            if (ReferenceEquals(null, value))
            {
                bsonValue = BsonNull.Value;
                return true;
            }

            if (value is DateTimeOffset offset)
            {
                bsonValue = new BsonDateTime(offset.UtcDateTime);
                return true;
            }

            bsonValue = null;
            return false;
        }
    }
}