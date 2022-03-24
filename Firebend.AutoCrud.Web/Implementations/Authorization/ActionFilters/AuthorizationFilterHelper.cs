using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;

public static class AuthorizationFilterHelper
{
    public static bool HandleAuthorizationResult(ActionExecutingContext context,
        AuthorizationResult authorizationResult)
    {
        if (!authorizationResult.Succeeded)
        {
            context.Result = ForbiddenResult(authorizationResult);
        }

        return authorizationResult.Succeeded;
    }

    public static bool HandleAuthorizationResult(ResultExecutingContext context,
        AuthorizationResult authorizationResult)
    {
        if (!authorizationResult.Succeeded)
        {
            context.Result = ForbiddenResult(authorizationResult);
        }

        return authorizationResult.Succeeded;
    }

    private static ObjectResult ForbiddenResult(AuthorizationResult authorizationResult) =>
        new ObjectResult(authorizationResult.Failure?.FailureReasons.FirstOrDefault()?.Message ?? "Forbidden")
        {
            StatusCode = 403
        };

    public static bool TryGetArgument<TArg>(ActionExecutingContext context, string argName, out TArg arg)
    {
        if (context.ActionArguments.TryGetValue(argName, out var paramValue))
        {
            arg = (TArg)paramValue;
            return true;
        }

        arg = default;
        return false;
    }
}
