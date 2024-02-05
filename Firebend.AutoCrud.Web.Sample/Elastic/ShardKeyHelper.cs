using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Models.DomainEvents;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using MassTransit;
using MassTransit.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Firebend.AutoCrud.Web.Sample.Elastic;

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
                var tenant = message.Message.EventContext?.GetCustomContext<SampleDomainEventContext>()?.Tenant;

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
        var tenantHeader = header![TenantHeader];

        if (tenantHeader.Count > 0)
        {
            return tenantHeader;
        }

        return Firebend;
    }

    public static readonly string[] AllShards = [
        Firebend,
        FirebendBackwards
    ];
}
