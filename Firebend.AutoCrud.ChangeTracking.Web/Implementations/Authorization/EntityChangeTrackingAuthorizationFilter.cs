using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;
using Humanizer;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Firebend.AutoCrud.ChangeTracking.Web.Implementations.Authorization;

public class EntityChangeTrackingAuthorizationFilter<TKey, TEntity> : EntityAuthorizationFilter<TKey, TEntity>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
{
    public EntityChangeTrackingAuthorizationFilter(string policy) : base(policy)
    {
    }

    protected override async Task AuthorizeRequestAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (await TryAuthorizeById(context, nameof(ChangeTrackingModel<TKey, TEntity>.EntityId).Camelize()))
        {
            await next();
        }
    }
}
