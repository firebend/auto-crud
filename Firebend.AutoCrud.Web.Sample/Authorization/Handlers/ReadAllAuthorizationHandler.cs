using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public class
    ReadAllAuthorizationHandler : AuthorizationHandler<ReadAllAuthorizationRequirement, IEnumerable<IEntityDataAuth>>
{
    private readonly DataAuthService _dataAuthService;

    public ReadAllAuthorizationHandler(DataAuthService dataAuthService)
    {
        _dataAuthService = dataAuthService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ReadAllAuthorizationRequirement requirement,
        IEnumerable<IEntityDataAuth> resources)
    {
        var entityDataAuths = resources as IEntityDataAuth[] ?? resources.ToArray();
        var authorizedResources = new List<IEntityDataAuth>();
        foreach (var entityDataAuth in entityDataAuths)
        {
            var isAuthorized = await _dataAuthService.AuthorizeAsync(context.User, entityDataAuth.DataAuth);
            if (!isAuthorized)
            {
                authorizedResources.Add(entityDataAuth);
            }
        }

        // TODO need to change response body
    }
}
