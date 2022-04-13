using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;

public static class AuthorizationFilterHelper
{

    /// <summary>
    /// Sets the context result to a 403 Forbidden with the associated failure reason (if exists) when the authorization result is a failure
    /// </summary>
    /// <param name="context">Action filter ActionExecutingContext</param>
    /// <param name="authorizationResult">AuthorizationResult used to determine if result should be Forbidden</param>
    public static bool HandleAuthorizationResult(ActionExecutingContext context,
        AuthorizationResult authorizationResult)
    {
        if (!authorizationResult.Succeeded)
        {
            context.Result = ForbiddenResult(authorizationResult);
        }

        return authorizationResult.Succeeded;
    }

    /// <summary>
    /// Sets the context result to a 403 Forbidden with the associated failure reason (if exists) when the authorization result is a failure
    /// </summary>
    /// <param name="context">Action filter ResultExecutingContext</param>
    /// <param name="authorizationResult">AuthorizationResult used to determine if result should be Forbidden</param>
    public static bool HandleAuthorizationResult(ResultExecutingContext context,
        AuthorizationResult authorizationResult)
    {
        if (!authorizationResult.Succeeded)
        {
            context.Result = ForbiddenResult(authorizationResult);
        }

        return authorizationResult.Succeeded;
    }

    /// <summary>
    /// Creates a 403 Forbidden Object result using the first failure reason (if exists) from the authorizationResult
    /// </summary>
    /// <param name="authorizationResult">AuthorizationResult used to determine forbidden message</param>
    public static ObjectResult ForbiddenResult(AuthorizationResult authorizationResult) =>
        new ObjectResult(authorizationResult.Failure?.FailureReasons.FirstOrDefault()?.Message ?? "Forbidden")
        {
            StatusCode = 403
        };

    /// <summary>
    /// Attempts to get and cast to generic type an argument from the context ActionArguments
    /// </summary>
    /// <param name="context">ActionExecutingContext that contains the ActionArguments</param>
    /// <param name="argName">Name of the argument to find</param>
    /// <param name="arg">Argument value if found and of type</param>
    public static bool TryGetArgument<TArg>(ActionExecutingContext context, string argName, out TArg arg)
    {
        if (context.ActionArguments.TryGetValue(argName, out var paramValue) && paramValue is TArg typedArg)
        {
            arg = typedArg;
            return true;
        }

        arg = default;
        return false;
    }

    /// <summary>
    /// Takes multiple authorize tasks and returns only the failed results
    /// </summary>
    /// <param name="tasks">AuthorizationResult tasks to execute</param>
    public static async Task<AuthorizationResult[]> AuthorizeMultiple(params Task<AuthorizationResult>[] tasks)
    {
        var authResults = await Task.WhenAll(tasks);
        return authResults.Where(r => !r.Succeeded).ToArray();
    }
}
