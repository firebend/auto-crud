using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using MassTransit.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Firebend.AutoCrud.Web.Sample.Elastic;

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
