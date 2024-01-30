using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Http;

namespace Firebend.AutoCrud.Web.Sample.Authorization.Handlers;

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
