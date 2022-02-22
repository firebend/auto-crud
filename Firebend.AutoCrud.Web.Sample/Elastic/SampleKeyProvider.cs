using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.Mongo.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Firebend.AutoCrud.Web.Sample.Elastic
{
    public static class ShardKeyHelper
    {
        private const string TenantHeader = "x-fb-ac-tenant";
        public const string Firebend = "Firebend";
        public const string FirebendBackwards = "Dneberif";

        public static string GetTenant(IHttpContextAccessor contextAccessor)
        {
            var header = contextAccessor?.HttpContext?.Request.Headers;
            if (header.IsEmpty())
            {
                return Firebend;
            }
            var tenantHeader = header[TenantHeader];

            if (tenantHeader.Count > 0)
            {
                return tenantHeader;
            }

            return Firebend;
        }
    }
    public class SampleKeyProvider : IShardKeyProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SampleKeyProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetShardKey() => ShardKeyHelper.GetTenant(_httpContextAccessor);
    }

    public class SampleKeyProviderMongo : IMongoShardKeyProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SampleKeyProviderMongo(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetShardKey() => ShardKeyHelper.GetTenant(_httpContextAccessor);
    }

    public class SampleAllShardsMongoProvider : IMongoAllShardsProvider
    {
        public Task<IEnumerable<string>> GetAllShardsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new[] { ShardKeyHelper.Firebend, ShardKeyHelper.FirebendBackwards }.AsEnumerable());
    }
}
