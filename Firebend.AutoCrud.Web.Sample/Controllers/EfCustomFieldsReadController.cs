using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.CustomFields;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Controllers;

[ApiController]
public class EfCustomFieldsReadController : ControllerBase
{
    private readonly ICustomFieldsReadService<Guid, EfPerson> _efCustomFieldsReadService;

    public EfCustomFieldsReadController(ICustomFieldsReadService<Guid, EfPerson> efCustomFieldsReadService)
    {
        _efCustomFieldsReadService = efCustomFieldsReadService;
    }

    [HttpGet]
    [Route("/api/v1/ef-person/{personId:Guid}/custom-fields")]
    public async Task<ActionResult<IEnumerable<CustomFieldViewModel>>> Get(Guid personId, [FromQuery] string key,
        CancellationToken cancellationToken)
    {
        var customFields = string.IsNullOrEmpty(key)
            ? await _efCustomFieldsReadService.GetAllAsync(personId, cancellationToken)
            : await _efCustomFieldsReadService.GetAllAsync(personId, cf => cf.Key == key, cancellationToken);

        return Ok(customFields.ToViewModel());
    }

    [HttpGet]
    [Route("/api/v1/ef-person/{personId:Guid}/custom-fields/{id:Guid}")]
    public async Task<ActionResult<CustomFieldViewModel>> GetById(Guid personId, Guid id,
        CancellationToken cancellationToken)
    {
        var customField = await _efCustomFieldsReadService.GetByKeyAsync(personId, id, cancellationToken);

        return Ok(customField.ToViewModel());
    }

    [HttpGet]
    [Route("/api/v1/ef-person/{personId:Guid}/custom-fields/exists")]
    public async Task<ActionResult<bool>> Exists(Guid personId, [FromQuery] string key,
        CancellationToken cancellationToken)
    {
        var result = await _efCustomFieldsReadService.ExistsAsync(personId, cf => cf.Key == key, cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    [Route("/api/v1/ef-person/{personId:Guid}/custom-fields/first")]
    public async Task<ActionResult<CustomFieldViewModel>> First(Guid personId, [FromQuery] string key,
        CancellationToken cancellationToken)
    {
        var result =
            await _efCustomFieldsReadService.FindFirstOrDefaultAsync(personId, cf => cf.Key == key, cancellationToken);

        return Ok(result.ToViewModel());
    }
}
