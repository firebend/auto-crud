using System;
using Firebend.AutoCrud.Core.Pooling;
using MongoDB.Bson.Serialization;

namespace Firebend.AutoCrud.Mongo.Configuration
{
    public class MongoIdGeneratorComb : IIdGenerator
    {
        public static readonly MongoIdGeneratorComb Instance = new();

        public object GenerateId(object container, object document) => NewCombGuid(Guid.NewGuid(), DateTime.UtcNow);

        public bool IsEmpty(object id) => id == default || (Guid)id == Guid.Empty;

        private static Guid NewCombGuid(Guid guid, DateTime timestamp)
        {
            var dateTime = DateTime.UnixEpoch;
            var timeSpan = timestamp - dateTime;
            var timeSpanMs = (long)timeSpan.TotalMilliseconds;
            var timestampString = timeSpanMs.ToString("x8");
            var guidString = guid.ToString("N");

            var pooledBuilder = AutoCrudObjectPool.StringBuilder.Get();

            string newGuidString;

            try
            {
                pooledBuilder.Append(timestampString[..11]);
                pooledBuilder.Append(guidString[11..]);
                newGuidString = pooledBuilder.ToString();
            }
            finally
            {
                AutoCrudObjectPool.StringBuilder.Return(pooledBuilder);
            }

            if (string.IsNullOrWhiteSpace(newGuidString))
            {
                throw new Exception("Could not get guid string");
            }

            var newGuid = Guid.Parse(newGuidString);

            return newGuid;
        }
    }
}
