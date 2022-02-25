using System.Collections.Generic;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class ReadAllAuthorizationHandler : AuthorizationHandler<ReadAllAuthorizationRequirement, IEnumerable<IDataAuth>>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ReadAllAuthorizationRequirement requirement,
        IEnumerable<IDataAuth> resource)
    {
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
