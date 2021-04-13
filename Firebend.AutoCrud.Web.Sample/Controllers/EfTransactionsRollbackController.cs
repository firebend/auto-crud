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
    [Route("/api/v1/ef/transactions/rollback")]
    public class EfTransactionsRollbackController : ControllerBase
    {
        private readonly IEntityTransactionFactory<Guid, EfPerson> _personTransactionFactory;
        private readonly IEntityReadService<Guid, EfPerson> _personRead;
        private readonly IEntityCreateService<Guid, EfPerson> _personCreate;

        public EfTransactionsRollbackController(IEntityTransactionFactory<Guid, EfPerson> personTransactionFactory,
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
            var person = new EfPerson {FirstName = "Transaction", LastName = "Rollback", NickName = "Mr. T"};
            using var transaction = await _personTransactionFactory.StartTransactionAsync(cancellationToken);
            Response.RegisterForDispose(transaction);

            EfPerson read;
            EfPerson createdRead;
            EfPerson created;

            try
            {
                read = await _personRead.FindFirstOrDefaultAsync(x => !x.IsDeleted, transaction, cancellationToken);
                created = await _personCreate.CreateAsync(person, transaction, cancellationToken);
                createdRead = await _personRead.GetByKeyAsync(created.Id, transaction, cancellationToken);
                await transaction.RollbackAsync(cancellationToken);
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return BadRequest(ex.ToString());
            }

            var readAgain = await _personRead.GetByKeyAsync(created.Id, cancellationToken);

            return Ok(new
            {
                ReadRandom = new GetPersonViewModel(read),
                Created = new GetPersonViewModel(created),
                CreatedRead = new GetPersonViewModel(createdRead),
                ReadAgain = new GetPersonViewModel(readAgain),
                DoesItExits = readAgain != null
            });
        }
    }
}
