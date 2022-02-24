using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.Handlers;

public class SearchAuthorizationHandler : AuthorizationHandler<SearchAuthorizationRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SearchAuthorizationRequirement requirement,
        Document resource) =>
        throw new System.NotImplementedException();
}

public class SearchAuthorizationRequirement : IAuthorizationRequirement { }
