using System.Linq;
using System.Security.Claims;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query.Internal;
using DbFunctionsExtensions = Firebend.AutoCrud.Web.Sample.DbContexts.DbFunctionsExtensions;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

public abstract class
    EntitySearchAuthorizationHandler<TKey, TEntity, TSearch> : IEntitySearchHandler<TKey, TEntity, TSearch>
    where TKey : struct
    where TEntity : IEntity<TKey>, IEntityDataAuth
    where TSearch : IEntitySearchRequest
{
    private readonly ClaimsPrincipal _user;

    public EntitySearchAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User;
    }


    public virtual IQueryable<TEntity> HandleSearch(IQueryable<TEntity> queryable, TSearch searchRequest)
    {
        var email = _user.Claims.FirstOrDefault(claim => claim.Type.EndsWith("emailaddress"))?.Value;
        if (string.IsNullOrEmpty(email))
        {
            // TODO this should return no results but unlikely to happen with successful authorization
            return queryable;
        }

        if (queryable is EntityQueryable<TEntity>)
        {
            return queryable.Where(x => x.DataAuth == null ||
                                        DbFunctionsExtensions.JsonArrayIsEmpty(nameof(x.DataAuth), "$.UserEmails") ||
                                        DbFunctionsExtensions.JsonValue(nameof(x.DataAuth), "$.UserEmails")
                                            .Contains(email));
        }

        return queryable.Where(x => x.DataAuth == null || x.DataAuth.UserEmails.Length == 0 ||
                                    x.DataAuth.UserEmails.Any(userEmail => userEmail.ToLower() == email));
    }
}

public class
    CustomFieldsSearchAuthorizationHandler<TKey, TEntity> : EntitySearchAuthorizationHandler<TKey, TEntity,
        CustomFieldsSearchRequest>
    where TKey : struct
    where TEntity : IEntity<TKey>, IEntityDataAuth
{
    public CustomFieldsSearchAuthorizationHandler(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }
}
