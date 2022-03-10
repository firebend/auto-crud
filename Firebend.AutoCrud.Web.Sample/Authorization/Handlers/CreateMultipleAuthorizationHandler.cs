using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class CreateMultipleAuthorizationHandler : AuthorizationHandler<CreateMultipleAuthorizationRequirement,
    IEnumerable<IEntityDataAuth>>
{
    private readonly DataAuthService _dataAuthService;

    public CreateMultipleAuthorizationHandler(DataAuthService dataAuthService)
    {
        _dataAuthService = dataAuthService;
    }

    public override async Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.Resource != null && context.Resource.GetType().GetInterfaces()
                .Any(i => i.FullName != null && i.FullName.Contains("IEntityViewModelCreateMultiple")))
        {
            foreach (var req in context.Requirements.OfType<CreateMultipleAuthorizationRequirement>())
            {
                var entities = context.Resource.GetType().GetProperty("Entities")?.GetValue(context.Resource);
                await HandleRequirementAsync(context, req, (IEnumerable<IEntityDataAuth>)entities);
            }
        }
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CreateMultipleAuthorizationRequirement requirement,
        IEnumerable<IEntityDataAuth> resources)
    {
        var entityDataAuths = resources as IEntityDataAuth[] ?? resources.ToArray();
        foreach (var entityDataAuth in entityDataAuths)
        {
            var isAuthorized = await _dataAuthService.AuthorizeAsync(context.User, entityDataAuth.DataAuth);
            if (entityDataAuth.DataAuth is null || !isAuthorized)
            {
                return;
            }
        }

        context.Succeed(requirement);
    }
}
