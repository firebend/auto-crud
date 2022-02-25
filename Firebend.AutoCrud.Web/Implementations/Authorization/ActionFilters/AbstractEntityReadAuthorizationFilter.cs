using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;

public class AbstractEntityReadAuthorizationFilter : IAsyncResultFilter
{
    private readonly string _policy;

    public AbstractEntityReadAuthorizationFilter(string policy)
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
            var entity = ((OkObjectResult)context.Result).Value;

            var authorizationResult =
                await authorizationService.AuthorizeAsync(context.HttpContext.User, entity, _policy);

            if (!authorizationResult.Succeeded)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
        }

        await next();
    }
}
