using System.Linq;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class PutAuthorizationHandler : AuthorizationHandler<UpdateAuthorizationRequirement, EntityViewModelCreate>
{
    private readonly DataAuthService _dataAuthService;

    public PutAuthorizationHandler(DataAuthService dataAuthService)
    {
        _dataAuthService = dataAuthService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdateAuthorizationRequirement requirement,
        EntityViewModelCreate resource)
    {
        if (await _dataAuthService.AuthorizeAsync(context.User, resource.Body.DataAuth))
        {
            context.Succeed(requirement);
        }
    }
}

public class PatchAuthorizationHandler : AuthorizationHandler<UpdateAuthorizationRequirement, IEntityDataAuth>
{
    private readonly DataAuthService _dataAuthService;

    public PatchAuthorizationHandler(DataAuthService dataAuthService)
    {
        _dataAuthService = dataAuthService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdateAuthorizationRequirement requirement,
        IEntityDataAuth resource)
    {
        if (await _dataAuthService.AuthorizeAsync(context.User, resource.DataAuth))
        {
            context.Succeed(requirement);
        }
    }
}
