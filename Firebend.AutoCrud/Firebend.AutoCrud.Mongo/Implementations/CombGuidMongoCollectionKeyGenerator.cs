using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Mongo.Interfaces;

namespace Firebend.AutoCrud.Mongo.Implementations
{
    public class CombGuidMongoCollectionKeyGenerator<TEntity> : IMongoCollectionKeyGenerator<TEntity, Guid>
        where TEntity : IEntity<Guid>
    {
        public Task<Guid> GenerateKeyAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetNewCombGuid(Guid.NewGuid(), DateTime.UtcNow));
        }
        
        private Guid GetNewCombGuid(Guid guid, DateTime timestamp)
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