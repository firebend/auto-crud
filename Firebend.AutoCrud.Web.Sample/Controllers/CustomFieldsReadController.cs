using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Controllers;

[ApiController]
public class CustomFieldsReadController : ControllerBase
{
    private readonly ICustomFieldsReadService<Guid, EfPerson> _efCustomFieldsReadService;

    public CustomFieldsReadController(ICustomFieldsReadService<Guid, EfPerson> efCustomFieldsReadService)
    {
        _efCustomFieldsReadService = efCustomFieldsReadService;
    }

    [HttpGet]
    [Route("/api/v1/ef-person/{personId:Guid}/custom-fields")]
    public async Task<ActionResult> Get(Guid personId, CancellationToken cancellationToken)
    {
        var customFields = await _efCustomFieldsReadService.GetAllAsync(personId, cancellationToken);

        return Ok(customFields);
    }

}
