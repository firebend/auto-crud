using System.Threading.Tasks;
using Firebend.AutoCrud.CustomFields.Web.Implementations.Authorization;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class CustomFieldsAuthorizationHandler : AuthorizationHandler<CustomFieldsAuthorizationRequirement, IEntityDataAuth>
{
    private readonly DataAuthService _dataAuthService;

    public CustomFieldsAuthorizationHandler(DataAuthService dataAuthService)
    {
        _dataAuthService = dataAuthService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomFieldsAuthorizationRequirement requirement,
        IEntityDataAuth resource)
    {
        if (await _dataAuthService.AuthorizeAsync(context.User, resource.DataAuth))
        {
            context.Succeed(requirement);
        }
    }
}
