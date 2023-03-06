using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.ChangeTracking.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization;

namespace Firebend.AutoCrud.Web.Sample.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("/api/v{version:apiVersion}/mongo/domain-event-custom-context")]
    public class MongoChangeTrackingCustomContextTestController : ControllerBase
    {
        static MongoChangeTrackingCustomContextTestController()
        {
            BsonClassMap.RegisterClassMap<CatchPhraseModel>();
        }

        private readonly IMongoReadClient<Guid, ChangeTrackingEntity<Guid, MongoTenantPerson>> _readClient;

        public MongoChangeTrackingCustomContextTestController(IMongoReadClient<Guid, ChangeTrackingEntity<Guid, MongoTenantPerson>> readClient)
        {
            _readClient = readClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
        {
            var all = await _readClient
                .GetAllAsync(x => x.DomainEventCustomContext != null, cancellationToken);

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
}
