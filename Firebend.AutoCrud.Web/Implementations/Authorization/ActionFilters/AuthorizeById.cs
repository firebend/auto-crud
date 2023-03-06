using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;

public class AuthorizeById<TKey, TEntity, TVersion> : EntityAuthorizationFilter<TKey, TEntity, TVersion>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
    where TVersion : class, IAutoCrudApiVersion
{
    private readonly string _idArgument;

    public AuthorizeById(string idArgument) : base(ReadAuthorizationRequirement.DefaultPolicy)
    {
        _idArgument = idArgument;
    }

    protected override async Task AuthorizeRequestAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (await TryAuthorizeById(context, _idArgument))
        {
            await next();
        }
    }
}
