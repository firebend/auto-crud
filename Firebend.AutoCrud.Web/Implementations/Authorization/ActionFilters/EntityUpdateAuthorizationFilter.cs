using System;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;

public class EntityUpdateAuthorizationFilter<TKey, TEntity> : IAsyncActionFilter
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public static readonly string[] RequiredProperties = {"ViewModelType"};
    private readonly string _policy;

    public Type ViewModelType { get; set; }

    public EntityUpdateAuthorizationFilter(string policy)
    {
        _policy = policy;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var readService = context.HttpContext.RequestServices.GetService<IEntityReadService<TKey, TEntity>>();
        var keyParser = context.HttpContext.RequestServices.GetService<IEntityKeyParser<TKey, TEntity>>();
        if (readService == null || keyParser == null)
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

        if (context.ActionArguments.TryGetValue("body", out var paramValue) && paramValue?.GetType() == ViewModelType)
        {
            var authorizationResult =
                await authorizationService.AuthorizeAsync(context.HttpContext.User, paramValue, _policy);

            if (!authorizationResult.Succeeded)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }
        }
        else if (context.HttpContext.Request.Method == HttpMethods.Patch &&
                 context.ActionArguments.TryGetValue(nameof(IEntity<TKey>.Id).ToLower(), out var idValue) &&
                 idValue is string entityIdString)
        {
            var entityId = keyParser.ParseKey(entityIdString);
            if (entityId == null)
            {
                await next();
                return;
            }

            var entity = await readService.GetByKeyAsync(entityId.Value, context.HttpContext.RequestAborted);
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
