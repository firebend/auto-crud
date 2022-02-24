using System.Reflection.Metadata;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class CreateAuthorizationHandler : AuthorizationHandler<CreateAuthorizationRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CreateAuthorizationRequirement requirement,
        Document resource)
    {
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

// change extensions as configurations on the sample project
// create an mvcbuilder extension for registering the action filters
