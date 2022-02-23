using System;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web.Implementations.Authorization;

public class DataAuthActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var memberInfo = context.Controller.GetType().BaseType;

        if (memberInfo == null)
        {
            await next();
            return;
        }

        var entityType = memberInfo.GenericTypeArguments[1];

        var serviceType = typeof(IEntityReadService<,>).MakeGenericType(typeof(Guid), entityType);

        dynamic readService = context.HttpContext.RequestServices.GetService(serviceType);

        if (readService == null)
        {
            await next();
            return;
        }

        var authorizationService = context.HttpContext.RequestServices.GetService<IAuthorizationService>();

        if (context.ActionArguments.TryGetValue("id", out var entityId))
        {
            var entity = await
                readService.GetByKeyAsync(Guid.Parse(((string)entityId)!), context.HttpContext.RequestAborted);

            Console.WriteLine("Retrieved instance of type " + entity.GetType());

            var authorizationResult =
                await authorizationService.AuthorizeAsync(context.HttpContext.User, (object)entity, Operations.Create);
            if (!authorizationResult.Succeeded)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }
        }
        await next();
    }
}
