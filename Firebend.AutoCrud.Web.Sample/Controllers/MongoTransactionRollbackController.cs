using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Sample.Models;
using Firebend.JsonPatch.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Controllers
{
    [ApiController]
    [Route("/api/v1/mongo/transactions/rollback")]
    public class MongoTransactionRollbackController : ControllerBase
    {
        private readonly IEntityTransactionFactory<Guid, MongoTenantPerson> _personTransactionFactory;
        private readonly IEntityReadService<Guid, MongoTenantPerson> _personRead;
        private readonly IEntityUpdateService<Guid, MongoTenantPerson> _personUpdate;
        private readonly IEntityCreateService<Guid, MongoTenantPerson> _personCreate;

        public MongoTransactionRollbackController(IEntityTransactionFactory<Guid, MongoTenantPerson> personTransactionFactory,
            IEntityReadService<Guid, MongoTenantPerson> personRead,
            IEntityUpdateService<Guid, MongoTenantPerson> personUpdate,
            IEntityCreateService<Guid, MongoTenantPerson> personCreate)
        {
            _personTransactionFactory = personTransactionFactory;
            _personRead = personRead;
            _personUpdate = personUpdate;
            _personCreate = personCreate;
        }

        [HttpPost]
        public async Task<ActionResult<EfPerson>> PostAsync(CancellationToken cancellationToken)
        {
            var person = new MongoTenantPerson { FirstName = "Transaction", LastName = "Test", IgnoreMe = "Mr T." };

            using var transaction = await _personTransactionFactory.StartTransactionAsync(cancellationToken);
            Response.RegisterForDispose(transaction);

            MongoTenantPerson read;
            MongoTenantPerson createdRead;
            MongoTenantPerson created;
            MongoTenantPerson patched;
            MongoTenantPerson updated;

            try
            {
                read = await _personRead.FindFirstOrDefaultAsync(x => !x.IsDeleted, transaction, cancellationToken);
                created = await _personCreate.CreateAsync(person, transaction, cancellationToken);
                createdRead = await _personRead.GetByKeyAsync(created.Id, transaction, cancellationToken);
                var patchDoc = new JsonPatchDocument<MongoTenantPerson>();
                patchDoc.Replace(x => x.LastName, "Test - Patch");
                patched = await _personUpdate.PatchAsync(read.Id, patchDoc, transaction, cancellationToken);
                var temp = patched.Clone();
                temp.LastName = "Test - Updated";
                updated = await _personUpdate.UpdateAsync(temp, transaction, cancellationToken);
                await transaction.RollbackAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest(ex.ToString());
            }

            read = await _personRead.GetByKeyAsync(read.Id, cancellationToken);
            var readAgain = await _personRead.GetByKeyAsync(created.Id, cancellationToken);

            return Ok(new
            {
                ReadRandom = read,
                Created = created,
                Patched = patched,
                Updated = updated,
                CreatedRead = createdRead,
                ReadAgain = readAgain,
                DoesItExits = readAgain != null
            });
        }
    }
}
