using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Sample.Models;
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

        public EfTransactionsController(IEntityTransactionFactory<Guid, EfPerson> personTransactionFactory,
            IEntityReadService<Guid, EfPerson> personRead,
            IEntityCreateService<Guid, EfPerson> personCreate)
        {
            _personTransactionFactory = personTransactionFactory;
            _personRead = personRead;
            _personCreate = personCreate;
        }

        [HttpPost]
        public async Task<ActionResult<EfPerson>> PostAsync(CancellationToken cancellationToken)
        {
            var person = new EfPerson {FirstName = "Transaction", LastName = "Test", NickName = "Mr. T"};
            using var transaction = await _personTransactionFactory.StartTransactionAsync(cancellationToken);
            Response.RegisterForDispose(transaction);
            try
            {
                var read = await _personRead.FindFirstOrDefaultAsync(x => !x.IsDeleted, cancellationToken);
                var created = await _personCreate.CreateAsync(person, transaction, cancellationToken);
                var createdRead = await _personRead.GetByKeyAsync(created.Id, cancellationToken);
                await transaction.CompleteAsync(cancellationToken);
                var readAgain = await _personRead.GetByKeyAsync(created.Id, cancellationToken);

                return Ok(new
                {
                    read,
                    created,
                    createdRead,
                    readAgain
                });
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest(ex.ToString());
            }
        }
    }
}
