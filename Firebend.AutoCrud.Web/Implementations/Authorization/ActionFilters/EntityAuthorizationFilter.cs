using System;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;

public abstract class EntityAuthorizationFilter<TKey, TEntity> : IAsyncActionFilter, IAsyncResultFilter
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    private readonly string _policy;
    private EntityAuthProvider _entityAuthProvider;

    protected EntityAuthorizationFilter(string policy)
    {
        _policy = policy;
    }

    public virtual Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _entityAuthProvider = context.HttpContext.RequestServices.GetService<EntityAuthProvider>();

        if (_entityAuthProvider == null)
        {
            throw new Exception($"Unable to resolve {nameof(EntityAuthProvider)}");
        }

        return AuthorizeRequestAsync(context, next);
    }

    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        _entityAuthProvider = context.HttpContext.RequestServices.GetService<EntityAuthProvider>();

        if (_entityAuthProvider == null)
        {
            throw new Exception($"Unable to resolve {nameof(EntityAuthProvider)}");
        }

        return AuthorizeResponseAsync(context, next);
    }

    protected virtual Task AuthorizeRequestAsync(ActionExecutingContext context, ActionExecutionDelegate next) => next();
    protected virtual Task AuthorizeResponseAsync(ResultExecutingContext context, ResultExecutionDelegate next) => next();

    protected async Task<bool> TryAuthorizeResponse(ResultExecutingContext context)
    {
        if (context.Result.GetType() != typeof(OkObjectResult))
        {
            return false;
        }

        var response = ((OkObjectResult)context.Result).Value;

        var authorizationResult =
            await _entityAuthProvider.AuthorizeEntityAsync(context.HttpContext.User, response, _policy);

        return AuthorizationFilterHelper.HandleAuthorizationResult(context, authorizationResult);
    }

    protected async Task<bool> TryAuthorizeById(ActionExecutingContext context, string idArgument)
    {
        if (!AuthorizationFilterHelper.TryGetArgument<string>(context, idArgument, out var entityIdString))
        {
            return false;
        }

        var authorizationResult =
            await _entityAuthProvider.AuthorizeEntityAsync<TKey, TEntity>(entityIdString, context.HttpContext.User,
                _policy, context.HttpContext.RequestAborted);

        return AuthorizationFilterHelper.HandleAuthorizationResult(context, authorizationResult);
    }

    protected async Task<bool> TryAuthorizeBody<TArg>(ActionExecutingContext context, string bodyArgument = "body")
    {
        if (!AuthorizationFilterHelper.TryGetArgument<TArg>(context, bodyArgument, out var body))
        {
            return false;
        }

        var authorizationResult =
            await _entityAuthProvider.AuthorizeEntityAsync(context.HttpContext.User, body, _policy);

        return AuthorizationFilterHelper.HandleAuthorizationResult(context, authorizationResult);
    }
}
