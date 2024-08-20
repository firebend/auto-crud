using System;
using System.Linq;
using Firebend.AutoCrud.Web.Sample.Authorization.Handlers;
using Microsoft.AspNetCore.Http;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class
    MongoCustomSearchHandler : EntitySearchAuthorizationHandler<Guid, MongoTenantPerson, CustomSearchParametersMongo>
{
    public MongoCustomSearchHandler(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }

    public override IQueryable<MongoTenantPerson> HandleSearch(IQueryable<MongoTenantPerson> queryable,
        CustomSearchParametersMongo searchRequest)
    {
        if (!string.IsNullOrWhiteSpace(searchRequest?.NickName))
        {
            queryable = queryable.Where(x => x.NickName == searchRequest.NickName);
        }

        return base.HandleSearch(queryable, searchRequest);
    }
}
