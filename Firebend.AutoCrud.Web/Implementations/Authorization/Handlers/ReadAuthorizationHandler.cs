using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Handlers;

public class ReadAuthorizationHandler : AuthorizationHandler<ReadAuthorizationRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ReadAuthorizationRequirement requirement,
        Document resource) =>
        throw new System.NotImplementedException();
}

public class ReadAuthorizationRequirement : IAuthorizationRequirement { }
