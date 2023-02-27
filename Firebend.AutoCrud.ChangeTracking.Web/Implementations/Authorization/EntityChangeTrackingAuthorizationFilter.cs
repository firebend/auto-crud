using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;
using Humanizer;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Firebend.AutoCrud.ChangeTracking.Web.Implementations.Authorization;

public class EntityChangeTrackingAuthorizationFilter<TKey, TEntity, TVersion> : EntityAuthorizationFilter<TKey, TEntity, TVersion>
    where TKey : struct
    where TEntity : class, IEntity<TKey>
    where TVersion : class, IApiVersion
{
    private static readonly string IdArg = nameof(ChangeTrackingModel<TKey, TEntity>.EntityId).Camelize();

    public EntityChangeTrackingAuthorizationFilter(string policy) : base(policy)
    {
    }

    protected override async Task AuthorizeRequestAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (await TryAuthorizeById(context, IdArg))
        {
            await next();
        }
    }
}
