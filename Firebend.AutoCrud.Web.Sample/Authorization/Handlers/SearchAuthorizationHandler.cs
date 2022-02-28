using System.Reflection.Metadata;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class SearchAuthorizationHandler : AuthorizationHandler<SearchAuthorizationRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SearchAuthorizationRequirement requirement,
        Document resource)
    {
        // Authorization business logic comes here
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
