using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class DeleteAuthorizationHandler : AuthorizationHandler<DeleteAuthorizationRequirement, IEntityDataAuth>
{
    private readonly DataAuthService _dataAuthService;

    public DeleteAuthorizationHandler(DataAuthService dataAuthService)
    {
        _dataAuthService = dataAuthService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DeleteAuthorizationRequirement requirement,
        IEntityDataAuth resource)
    {
        if (resource.DataAuth is not null &&
            await _dataAuthService.AuthorizeAsync(context.User, resource.DataAuth))
        {
            context.Succeed(requirement);
        }
    }
}
