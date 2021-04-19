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
    [Route("/api/v1/ef/transactions/commit")]
    public class EfTransactionsController : ControllerBase
    {
        private readonly IEntityTransactionFactory<Guid, EfPerson> _personTransactionFactory;
        private readonly IEntityReadService<Guid, EfPerson> _personRead;
        private readonly IEntityCreateService<Guid, EfPerson> _personCreate;
        private readonly IEntityUpdateService<Guid, EfPerson> _personUpdate;

        public EfTransactionsController(IEntityTransactionFactory<Guid, EfPerson> personTransactionFactory,
            IEntityReadService<Guid, EfPerson> personRead,
            IEntityCreateService<Guid, EfPerson> personCreate,
            IEntityUpdateService<Guid, EfPerson> personUpdate)
        {
            _personTransactionFactory = personTransactionFactory;
            _personRead = personRead;
            _personCreate = personCreate;
            _personUpdate = personUpdate;
        }

        [HttpPost]
        public async Task<ActionResult<EfPerson>> PostAsync(CancellationToken cancellationToken)
        {
            var person = new EfPerson { FirstName = "Transaction", LastName = "Test", NickName = "Mr. T" };
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
                createdRead = await _personRead.GetByKeyAsync(created.Id, transaction, cancellationToken);
                var patchDoc = new JsonPatchDocument<EfPerson>();
                patchDoc.Replace(x => x.LastName, "Test - Patch");
                patched = await _personUpdate.PatchAsync(created.Id, patchDoc, transaction, cancellationToken);
                var temp = patched.Clone();
                temp.LastName = "Test - Updated";
                updated = await _personUpdate.UpdateAsync(temp, transaction, cancellationToken);
                await transaction.CompleteAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest(ex.ToString());
            }

            var readAgain = await _personRead.GetByKeyAsync(created.Id, cancellationToken);

            return Ok(new
            {
                ReadRandom = new GetPersonViewModel(read),
                created,
                createdRead,
                patched,
                updated,
                readAgain,
            });
        }
    }
}
