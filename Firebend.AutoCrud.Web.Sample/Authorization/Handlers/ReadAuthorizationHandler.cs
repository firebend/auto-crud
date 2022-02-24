using System.Reflection.Metadata;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class ReadAuthorizationHandler : AuthorizationHandler<ReadAuthorizationRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ReadAuthorizationRequirement requirement,
        Document resource)
    {
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
