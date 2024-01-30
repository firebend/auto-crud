using Firebend.AutoCrud.Mongo.Interfaces;
using MassTransit.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Firebend.AutoCrud.Web.Sample.Elastic;

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
