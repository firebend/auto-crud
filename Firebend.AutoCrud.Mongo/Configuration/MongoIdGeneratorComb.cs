using System;
using System.Text;
using MongoDB.Bson.Serialization;

namespace Firebend.AutoCrud.Mongo.Configuration
{
    public class MongoIdGeneratorComb : IIdGenerator
    {
        public static readonly MongoIdGeneratorComb Instance = new MongoIdGeneratorComb();

        public object GenerateId(object container, object document) => NewCombGuid(Guid.NewGuid(), DateTime.UtcNow);

        public bool IsEmpty(object id) => id == default || (Guid)id == Guid.Empty;

        private static Guid NewCombGuid(Guid guid, DateTime timestamp)
        {
            var dateTime = DateTime.UnixEpoch;
            var timeSpan = timestamp - dateTime;
            var timeSpanMs = (long)timeSpan.TotalMilliseconds;
            var timestampString = timeSpanMs.ToString("x8");
            var guidString = guid.ToString("N");
            var guidStringBuilder = new StringBuilder(timestampString.Substring(0, 11));
            guidStringBuilder.Append(guidString.Substring(11));
            var newGuidString = guidStringBuilder.ToString();
            var newGuid = Guid.Parse(newGuidString);

            return newGuid;
        }
    }
}
