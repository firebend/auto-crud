using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("/api/v{version:apiVersion}/ef/domain-event-custom-context")]
public class EfChangeTrackingCustomContextTestController : ControllerBase
{
    private readonly IEntityFrameworkQueryClient<Guid, ChangeTrackingEntity<Guid, EfPerson>> _readClient;

    public EfChangeTrackingCustomContextTestController(IEntityFrameworkQueryClient<Guid, ChangeTrackingEntity<Guid, EfPerson>> readClient)
    {
        _readClient = readClient;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var all = await _readClient
            .GetAllAsync(x => x.DomainEventCustomContext != null, true, cancellationToken);

        return Ok(new
        {
            all,
            catchPhrases = all
                .Select(x => x.GetDomainEventContext<CatchPhraseModel>()?.CatchPhrase)
                .Distinct()
                .ToArray()
        });

    }
}
