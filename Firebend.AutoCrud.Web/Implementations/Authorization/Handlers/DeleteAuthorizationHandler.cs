using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Handlers;

public class DeleteAuthorizationHandler : AuthorizationHandler<DeleteAuthorizationRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DeleteAuthorizationRequirement requirement,
        Document resource) =>
        throw new System.NotImplementedException();
}

public class DeleteAuthorizationRequirement : IAuthorizationRequirement { }
