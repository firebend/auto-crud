using System;
using System.Linq;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.EntityFramework.CustomCommands;
using Firebend.AutoCrud.Web.Sample.Authorization.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class CustomSearchParameters : ModifiedEntitySearchRequest
{
    public string NickName { get; set; }
}

public class
    MongoCustomSearchHandler : EntitySearchAuthorizationHandler<Guid, MongoTenantPerson, CustomSearchParameters>
{
    public MongoCustomSearchHandler(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }

    public override IQueryable<MongoTenantPerson> HandleSearch(IQueryable<MongoTenantPerson> queryable,
        CustomSearchParameters searchRequest)
    {
        if (!string.IsNullOrWhiteSpace(searchRequest?.NickName))
        {
            queryable = queryable.Where(x => x.NickName == searchRequest.NickName);
        }

        return base.HandleSearch(queryable, searchRequest);
    }
}

public class EfCustomSearchHandler : EntitySearchAuthorizationHandler<Guid, EfPerson, CustomSearchParameters>
{
    public EfCustomSearchHandler(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
    {
    }

    public override IQueryable<EfPerson> HandleSearch(IQueryable<EfPerson> queryable,
        CustomSearchParameters searchRequest)
    {
        if (!string.IsNullOrWhiteSpace(searchRequest?.NickName))
        {
            queryable = queryable.Where(x => x.NickName == searchRequest.NickName);
        }

        if (!string.IsNullOrWhiteSpace(searchRequest?.Search))
        {
            queryable = queryable.Where(x => EF.Functions.ContainsAny(x.FirstName, searchRequest.Search));
        }

        return base.HandleSearch(queryable, searchRequest);
    }
}

public class PetSearch : EntitySearchRequest
{
    [FromRoute] public Guid? PersonId { get; set; }
}
