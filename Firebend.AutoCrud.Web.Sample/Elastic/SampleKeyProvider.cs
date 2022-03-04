using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using MassTransit;
using MassTransit.Scoping;
using Microsoft.AspNetCore.Http;

namespace Firebend.AutoCrud.Web.Sample.Elastic
{
    public static class ShardKeyHelper
    {
        private const string TenantHeader = "x-fb-ac-tenant";
        public const string Firebend = "Firebend";
        public const string FirebendBackwards = "Dneberif";

        public static string GetTenant(IHttpContextAccessor contextAccessor, ScopedConsumeContextProvider scoped)
        {
            ///********************************************
            // Author: JMA
            // Date: 2022-03-01 10:33:45
            // Comment: If we are operating on the service bus, resolve our custom tenant context
            //*******************************************
            if (scoped.HasContext)
            {
                if (scoped.GetContext()?.TryGetMessage(out ConsumeContext<DomainEventBase> message) ?? false)
                {
                    var tenant = message.Message?.EventContext?.GetCustomContext<SampleDomainEventContext>()?.Tenant;

                    if (!string.IsNullOrWhiteSpace(tenant))
                    {
                        return tenant;
                    }
                }
            }

            //********************************************
            // Author: JMA
            // Date: 2022-03-01 10:34:03
            // Comment: Else if we are on the http pipeline resolve our tenant from headers
            //*******************************************
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
        private readonly ScopedConsumeContextProvider _scopedConsumeContextProvider;

        public SampleKeyProvider(IHttpContextAccessor httpContextAccessor,
            ScopedConsumeContextProvider scopedConsumeContextProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _scopedConsumeContextProvider = scopedConsumeContextProvider;
        }

        public string GetShardKey() => ShardKeyHelper.GetTenant(_httpContextAccessor, _scopedConsumeContextProvider);
    }

    public class SampleKeyProviderMongo : IMongoShardKeyProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ScopedConsumeContextProvider _scopedConsumeContextProvider;

        public SampleKeyProviderMongo(IHttpContextAccessor httpContextAccessor,
            ScopedConsumeContextProvider scopedConsumeContextProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _scopedConsumeContextProvider = scopedConsumeContextProvider;
        }

        public string GetShardKey() => ShardKeyHelper.GetTenant(_httpContextAccessor, _scopedConsumeContextProvider);
    }

    public class SampleAllShardsMongoProvider : IMongoAllShardsProvider
    {
        public Task<IEnumerable<string>> GetAllShardsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new[] { ShardKeyHelper.Firebend, ShardKeyHelper.FirebendBackwards }.AsEnumerable());
    }
}
