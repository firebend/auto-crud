using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class CreateMultipleAuthorizationHandler : AuthorizationHandler<CreateMultipleAuthorizationRequirement, IDataAuth>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CreateMultipleAuthorizationRequirement requirement,
        IDataAuth resource)
    {
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
