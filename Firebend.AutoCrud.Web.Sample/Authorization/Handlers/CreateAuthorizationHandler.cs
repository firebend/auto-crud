using System.Reflection.Metadata;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class CreateAuthorizationHandler : AuthorizationHandler<CreateAuthorizationRequirement, IDataAuth>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CreateAuthorizationRequirement requirement,
        IDataAuth resource)
    {
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
