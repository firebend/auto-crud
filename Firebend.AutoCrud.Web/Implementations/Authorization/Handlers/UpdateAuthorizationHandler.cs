using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Handlers;

public class UpdateAuthorizationHandler : AuthorizationHandler<UpdateAuthorizationRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdateAuthorizationRequirement requirement,
        Document resource) =>
        throw new System.NotImplementedException();
}

public class UpdateAuthorizationRequirement : IAuthorizationRequirement { }
