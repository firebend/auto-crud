using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class DeleteAuthorizationHandler : AuthorizationHandler<DeleteAuthorizationRequirement, IEntityDataAuth>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DeleteAuthorizationRequirement requirement,
        IEntityDataAuth resource)
    {
        // Authorization business logic comes here
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
