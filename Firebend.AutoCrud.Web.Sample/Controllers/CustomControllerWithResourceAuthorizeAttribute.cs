using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Attributes;
using Firebend.AutoCrud.Web.Implementations.Authorization.ActionFilters;
using Firebend.AutoCrud.Web.Sample.Extensions;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Firebend.AutoCrud.Web.Sample.Controllers;

[Authorize]
[Route("/api/v1/mongo-person/{personId:guid}/fullname")]
[OpenApiGroupName("The Beautiful Mongo People")]
[OpenApiEntityName("MongoTenantPerson")]
[ApiController]
public class CustomControllerWithResourceAuthorizeAttribute : AbstractEntityControllerBase<V1>
{
    private readonly IEntityReadService<Guid, MongoTenantPerson> _readService;

    public CustomControllerWithResourceAuthorizeAttribute(IOptions<ApiBehaviorOptions> apiOptions,
        IEntityReadService<Guid, MongoTenantPerson> readService) : base(apiOptions)
    {
        _readService = readService;
    }

    [HttpGet]
    [Produces("application/json")]
    [TypeFilter(typeof(AuthorizeById<Guid, MongoTenantPerson, V1>),
        Arguments = new object[] { "personId" })]
    public async Task<IActionResult> GetFullName(
        [FromRoute] Guid personId,
        CancellationToken cancellationToken)
    {
        if (personId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(MongoPerson), "Please provide a valid shipment id");
            return GetInvalidModelStateResult();
        }

        var entity = await _readService.GetByKeyAsync(personId, cancellationToken);

        if (entity == null)
        {
            return NotFound();
        }

        return Ok($"{entity.FirstName} {entity.LastName}");
    }
}
