using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Firebend.AutoCrud.CustomFields.Web.Implementations.Authorization;

public class CustomFieldsAuthorizationFilter<TKey, TEntity> : EntityAuthorizationFilter<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public CustomFieldsAuthorizationFilter(string policy) : base(policy)
    {
    }

    protected override async Task AuthorizeRequestAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if(await TryAuthorizeById(context, nameof(CustomFieldsEntity<TKey>.EntityId)))
        {
            await next();
        }
    }
}
