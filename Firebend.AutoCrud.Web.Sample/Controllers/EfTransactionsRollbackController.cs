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
    [Route("/api/v1/ef/transactions/rollback")]
    public class EfTransactionsRollbackController : ControllerBase
    {
        private readonly IEntityTransactionFactory<Guid, EfPerson> _personTransactionFactory;
        private readonly IEntityReadService<Guid, EfPerson> _personRead;
        private readonly IEntityUpdateService<Guid, EfPerson> _personUpdate;
        private readonly IEntityCreateService<Guid, EfPerson> _personCreate;

        public EfTransactionsRollbackController(IEntityTransactionFactory<Guid, EfPerson> personTransactionFactory,
            IEntityReadService<Guid, EfPerson> personRead,
            IEntityUpdateService<Guid, EfPerson> personUpdate,
            IEntityCreateService<Guid, EfPerson> personCreate)
        {
            _personTransactionFactory = personTransactionFactory;
            _personRead = personRead;
            _personUpdate = personUpdate;
            _personCreate = personCreate;
        }

        [HttpPost]
        public async Task<ActionResult<EfPerson>> PostAsync(CancellationToken cancellationToken)
        {
            var person = new EfPerson { FirstName = "Transaction", LastName = "Rollback", NickName = "Mr. T" };
            using var transaction = await _personTransactionFactory.StartTransactionAsync(cancellationToken);
            Response.RegisterForDispose(transaction);

            EfPerson read;
            EfPerson createdRead;
            EfPerson created;
            EfPerson patched;
            EfPerson updated;

            try
            {
                read = await _personRead.FindFirstOrDefaultAsync(x => !x.IsDeleted, transaction, cancellationToken);
                created = await _personCreate.CreateAsync(person, transaction, cancellationToken);
                var patchDoc = new JsonPatchDocument<EfPerson>();
                patchDoc.Replace(x => x.FirstName, "Test - Patch");
                patched = await _personUpdate.PatchAsync(read.Id, patchDoc, transaction, cancellationToken);
                var temp = patched.Clone();
                temp.LastName = "Test - Updated";
                updated = await _personUpdate.UpdateAsync(temp, transaction, cancellationToken);
                createdRead = await _personRead.GetByKeyAsync(read.Id, transaction, cancellationToken);

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
                ReadRandom = new GetPersonViewModel(read),
                Created = new GetPersonViewModel(created),
                Patched = new GetPersonViewModel(patched),
                Updated = new GetPersonViewModel(updated),
                CreatedRead = new GetPersonViewModel(createdRead),
                ReadAgain = new GetPersonViewModel(readAgain),
                DoesItExits = readAgain != null
            });
        }
    }
}
