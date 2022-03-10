using System.Linq;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class CreateAuthorizationHandler : AuthorizationHandler<CreateAuthorizationRequirement,
    IEntityDataAuth>
{
    private readonly DataAuthService _dataAuthService;

    public CreateAuthorizationHandler(DataAuthService dataAuthService)
    {
        _dataAuthService = dataAuthService;
    }

    public override async Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.Resource != null && context.Resource.GetType().GetInterfaces()
                .Any(i => i.FullName != null && i.FullName.Contains("IEntityViewModelCreate")))
        {
            foreach (var req in context.Requirements.OfType<CreateAuthorizationRequirement>())
            {
                var body = context.Resource.GetType().GetProperty("Body")?.GetValue(context.Resource);
                await HandleRequirementAsync(context, req, (IEntityDataAuth)body);
            }
        }
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CreateAuthorizationRequirement requirement,
        IEntityDataAuth resource)
    {
        if (resource.DataAuth is not null &&
            await _dataAuthService.AuthorizeAsync(context.User, resource.DataAuth))
        {
            context.Succeed(requirement);
        }
    }
}
