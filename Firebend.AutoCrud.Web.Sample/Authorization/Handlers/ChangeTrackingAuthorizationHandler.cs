using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Web.Implementations.Authorization;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class ChangeTrackingAuthorizationHandler : AuthorizationHandler<ChangeTrackingAuthorizationRequirement, IEntityDataAuth>
{
    private readonly DataAuthService _dataAuthService;

    public ChangeTrackingAuthorizationHandler(DataAuthService dataAuthService)
    {
        _dataAuthService = dataAuthService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ChangeTrackingAuthorizationRequirement requirement,
        IEntityDataAuth resource)
    {
        if (await _dataAuthService.AuthorizeAsync(context.User, resource.DataAuth))
        {
            context.Succeed(requirement);
        }
    }
}
