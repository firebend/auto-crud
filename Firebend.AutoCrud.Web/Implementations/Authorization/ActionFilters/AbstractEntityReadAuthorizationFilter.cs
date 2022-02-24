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

public class AbstractEntityReadAuthorizationFilter<TKey, TEntity> : IAsyncActionFilter
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private IEnumerable<IAuthorizationRequirement> _requirements;

    public AbstractEntityReadAuthorizationFilter(IEnumerable<IAuthorizationRequirement> requirements)
    {
        _requirements = requirements;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next();

        var authorizationService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();

        if (authorizationService == null)
        {
            await next();
            return;
        }

        var entity = (TEntity)default; // TODO get entity from response

        var authorizationResult =
            await authorizationService.AuthorizeAsync(context.HttpContext.User, entity, _requirements);

        if (!authorizationResult.Succeeded)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            return;
        }

        await next();
    }
}
