using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class CreateMultipleAuthorizationHandler : AuthorizationHandler<CreateMultipleAuthorizationRequirement, EntityViewModelCreate>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CreateMultipleAuthorizationRequirement requirement,
        EntityViewModelCreate resource)
    {
        // Authorization business logic comes here
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
