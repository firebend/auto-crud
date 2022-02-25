using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;

public class AbstractEntityDeleteAuthorizationFilter<TKey, TEntity> : IAsyncActionFilter
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly string _policy;

    public AbstractEntityDeleteAuthorizationFilter(string policy)
    {
        _policy = policy;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var readService = context.HttpContext.RequestServices.GetService<IEntityReadService<TKey, TEntity>>();
        if (readService == null)
        {
            await next();
            return;
        }

        var authorizationService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();
        if (authorizationService == null)
        {
            await next();
            return;
        }

        if (context.ActionArguments.TryGetValue(nameof(IEntity<TKey>.Id).ToLower(), out var paramValue) && paramValue is TKey entityId)
        {
            var entity = await
                readService.GetByKeyAsync(entityId, context.HttpContext.RequestAborted);

            var authorizationResult =
                await authorizationService.AuthorizeAsync(context.HttpContext.User, entity, _policy);

            if (!authorizationResult.Succeeded)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }
        }

        await next();
    }
}
