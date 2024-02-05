using System;
using System.Linq;
using Firebend.AutoCrud.EntityFramework.CustomCommands;
using Firebend.AutoCrud.Web.Sample.Authorization.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.Web.Sample.Models;

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
