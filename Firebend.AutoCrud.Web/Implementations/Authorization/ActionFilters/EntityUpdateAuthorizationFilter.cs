using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;

public class EntityUpdateAuthorizationFilter<TKey, TEntity, TViewModel> : EntityAuthorizationFilter<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public EntityUpdateAuthorizationFilter(string policy) : base(policy)
    {
    }

    protected override async Task AuthorizeRequestAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (await TryAuthorizeBody<TViewModel>(context))
        {
            await next();
            return;
        }

        if(await TryAuthorizeById(context, nameof(IEntity<TKey>.Id)))
        {
            await next();
        }
    }
}
