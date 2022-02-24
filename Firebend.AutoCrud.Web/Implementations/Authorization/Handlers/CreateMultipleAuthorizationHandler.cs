using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Handlers;

public class CreateMultipleAuthorizationHandler : AuthorizationHandler<CreateAuthorizationRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CreateAuthorizationRequirement requirement,
        Document resource) =>
        throw new System.NotImplementedException();
}

public class CreateMultipleAuthorizationRequirement : IAuthorizationRequirement { }
