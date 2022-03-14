using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.ChangeTracking.Web.Implementations.Authorization;

public class EntityChangeTrackingAuthorizationFilter<TKey, TEntity> : IAsyncActionFilter
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly string _policy;

    public EntityChangeTrackingAuthorizationFilter(string policy)
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

        if (context.ActionArguments.TryGetValue(nameof(ChangeTrackingModel<TKey, TEntity>.EntityId), out var paramValue) &&
            paramValue is string entityIdString)
        {
            var entityId = keyParser.ParseKey(entityIdString);
            if (entityId == null)
            {
                await next();
                return;
            }

            var entity = await
                readService.GetByKeyAsync(entityId.Value, context.HttpContext.RequestAborted);

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
