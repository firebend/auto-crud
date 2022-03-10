using System.Linq;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class PutAuthorizationHandler : AuthorizationHandler<UpdateAuthorizationRequirement,
    IEntityDataAuth>
{
    private readonly DataAuthService _dataAuthService;

    public PutAuthorizationHandler(DataAuthService dataAuthService)
    {
        _dataAuthService = dataAuthService;
    }

    public override async Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.Resource != null && context.Resource.GetType().GetInterfaces()
                .Any(i => i.FullName != null && i.FullName.Contains("IEntityViewModelCreate")))
        {
            foreach (var req in context.Requirements.OfType<UpdateAuthorizationRequirement>())
            {
                var body = context.Resource.GetType().GetProperty("Body")?.GetValue(context.Resource);
                await HandleRequirementAsync(context, req, (IEntityDataAuth)body);
            }
        }
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
