using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;

public class EntityReadAuthorizationFilter<TKey, TEntity> : EntityAuthorizationFilter<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public EntityReadAuthorizationFilter(string policy) : base(policy)
    {
    }

    protected override async Task AuthorizeResponseAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await TryAuthorizeResponse(context);
        await next();
    }
}
