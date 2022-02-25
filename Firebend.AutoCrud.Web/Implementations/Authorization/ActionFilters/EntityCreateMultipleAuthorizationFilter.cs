using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;

public class EntityCreateMultipleAuthorizationFilter : IAsyncActionFilter
{
    public static readonly string[] RequiredProperties = {"ViewModelType"};
    private readonly string _policy;

    public Type ViewModelType { get; set; }

    public EntityCreateMultipleAuthorizationFilter(string policy)
    {
        _policy = policy;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
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

        await next();
    }
}
