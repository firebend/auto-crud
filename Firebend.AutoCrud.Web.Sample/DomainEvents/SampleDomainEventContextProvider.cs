using Firebend.AutoCrud.Core.Interfaces.Services.DomainEvents;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Elastic;
using MassTransit.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Firebend.AutoCrud.Web.Sample.DomainEvents;

public class SampleDomainEventContextProvider : IDomainEventContextProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ScopedConsumeContextProvider _scopedConsumeContextProvider;

    public SampleDomainEventContextProvider(IHttpContextAccessor httpContextAccessor,
        ScopedConsumeContextProvider scopedConsumeContextProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _scopedConsumeContextProvider = scopedConsumeContextProvider;
    }

    public DomainEventContext GetContext() => new()
    {
        Source = "My Sample",
        UserEmail = "sample@firebend.com",
        CustomContext = new SampleDomainEventContext
        {
            CatchPhraseModel = new CatchPhraseModel
            {
                CatchPhrase = "I Like Turtles",
            },
            Tenant = ShardKeyHelper.GetTenant(_httpContextAccessor, _scopedConsumeContextProvider)
        }
    };
}
