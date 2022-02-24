using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Handlers;

public class ReadAllAuthorizationHandler : AuthorizationHandler<ReadAllAuthorizationRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ReadAllAuthorizationRequirement requirement,
        Document resource) =>
        throw new System.NotImplementedException();
}

public class ReadAllAuthorizationRequirement : IAuthorizationRequirement { }
