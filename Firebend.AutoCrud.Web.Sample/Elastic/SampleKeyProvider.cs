using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using MassTransit.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Firebend.AutoCrud.Web.Sample.Elastic;

public class SampleKeyProvider : IShardKeyProvider, IAllShardKeyProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ScopedConsumeContextProvider _scopedConsumeContextProvider;

    public SampleKeyProvider(IHttpContextAccessor httpContextAccessor,
        ScopedConsumeContextProvider scopedConsumeContextProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _scopedConsumeContextProvider = scopedConsumeContextProvider;
    }

    public Task<string> GetShardKeyAsync(CancellationToken cancellationToken) => Task.FromResult(ShardKeyHelper.GetTenant(_httpContextAccessor, _scopedConsumeContextProvider));
    public Task<string[]> GetAllShards(CancellationToken cancellationToken) => Task.FromResult(ShardKeyHelper.AllShards);
}
