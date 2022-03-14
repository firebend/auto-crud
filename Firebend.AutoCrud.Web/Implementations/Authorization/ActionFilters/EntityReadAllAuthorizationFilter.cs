using System.Collections.Generic;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;

public class EntityReadAllAuthorizationFilter : IAsyncResultFilter
{
    private readonly string _policy;

    public EntityReadAllAuthorizationFilter(string policy)
    {
        _policy = policy;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var authorizationService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();

        if (authorizationService == null)
        {
            await next();
            return;
        }

        if (context.Result.GetType() == typeof(OkObjectResult))
        {
            var entities = ((OkObjectResult)context.Result).Value;

            var authorizationResult =
                await authorizationService.AuthorizeAsync(context.HttpContext.User, entities, _policy);

            if (!authorizationResult.Succeeded)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
        }

        await next();
    }
}

