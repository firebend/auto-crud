using System.Linq;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class CreateAuthorizationHandler : AuthorizationHandler<CreateAuthorizationRequirement, EntityViewModelCreate>
{
    private readonly DataAuthService _dataAuthService;

    public CreateAuthorizationHandler(DataAuthService dataAuthService)
    {
        _dataAuthService = dataAuthService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CreateAuthorizationRequirement requirement,
        EntityViewModelCreate resource)
    {
        if (resource.Body.DataAuth is not null &&
            await _dataAuthService.AuthorizeAsync(context.User, resource.Body.DataAuth))
        {
            context.Succeed(requirement);
        }
    }
}
