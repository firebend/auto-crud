using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Sample.Extensions;
using Firebend.AutoCrud.Web.Sample.Models;
using Firebend.JsonPatch.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Web.Sample.Controllers;

[ApiController]
public class SessionManagedTransactionsController : ControllerBase
{
    private readonly ISessionTransactionManager _transactionManager;
    private readonly IEntityReadService<Guid, EfPerson> _efPersonRead;
    private readonly IEntityCreateService<Guid, EfPerson> _efPersonCreate;
    private readonly IEntityUpdateService<Guid, EfPerson> _efPersonUpdate;
    private readonly IEntityDeleteService<Guid, EfPerson> _efPersonDelete;
    private readonly IEntityReadService<Guid, MongoTenantPerson> _mongoPersonRead;
    private readonly IEntityUpdateService<Guid, MongoTenantPerson> _mongoPersonUpdate;
    private readonly IEntityCreateService<Guid, MongoTenantPerson> _mongoPersonCreate;
    private readonly IEntityDeleteService<Guid, MongoTenantPerson> _mongoPersonDelete;

    public SessionManagedTransactionsController(ISessionTransactionManager transactionManager,
        IEntityReadService<Guid, EfPerson> efPersonRead,
        IEntityCreateService<Guid, EfPerson> efPersonCreate,
        IEntityUpdateService<Guid, EfPerson> efPersonUpdate,
        IEntityDeleteService<Guid, EfPerson> efPersonDelete,
        IEntityReadService<Guid, MongoTenantPerson> mongoPersonRead,
        IEntityUpdateService<Guid, MongoTenantPerson> mongoPersonUpdate,
        IEntityCreateService<Guid, MongoTenantPerson> mongoPersonCreate,
        IEntityDeleteService<Guid, MongoTenantPerson> mongoPersonDelete)
    {
        _transactionManager = transactionManager;
        _efPersonRead = efPersonRead;
        _efPersonCreate = efPersonCreate;
        _efPersonUpdate = efPersonUpdate;
        _efPersonDelete = efPersonDelete;
        _mongoPersonRead = mongoPersonRead;
        _mongoPersonUpdate = mongoPersonUpdate;
        _mongoPersonCreate = mongoPersonCreate;
        _mongoPersonDelete = mongoPersonDelete;
    }

    [HttpPost]
    [Route("/api/v1/transactions/commit")]
    public Task<ActionResult<SessionTransactionAssertionViewModel>> CommitAsync(
        [FromBody] SessionTransactionRequestModel requestModel, CancellationToken cancellationToken)
    {
        _transactionManager.Start();
        return TestCrudOperations(requestModel, _transactionManager.CompleteAsync, cancellationToken);
    }

    [HttpPost]
    [Route("/api/v1/transactions/rollback")]
    public Task<ActionResult<SessionTransactionAssertionViewModel>> RollbackAsync(
        [FromBody] SessionTransactionRequestModel requestModel, CancellationToken cancellationToken)
    {
        _transactionManager.Start();
        return TestCrudOperations(requestModel, _transactionManager.RollbackAsync, cancellationToken);
    }

    [HttpPost]
    [Route("/api/v1/transactions/exception")]
    public Task<ActionResult<SessionTransactionAssertionViewModel>> ExceptionRollbackAsync(
        [FromBody] SessionTransactionRequestModel requestModel,
        CancellationToken cancellationToken)
    {
        _transactionManager.Start();
        return TestCrudOperations(requestModel, null, cancellationToken);
    }

    private async Task<ActionResult<SessionTransactionAssertionViewModel>> TestCrudOperations(
        SessionTransactionRequestModel requestModel,
        Func<CancellationToken, Task> transactionAction,
        CancellationToken cancellationToken)
    {
        var testEfCreate = new EfPerson { FirstName = "Transaction", LastName = "Test", NickName = "Mr. T" };
        var testMongoCreate = new MongoTenantPerson { FirstName = "Transaction", LastName = "Test", IgnoreMe = "Mr T." };
        var testPatchValue = "Test - Patch";
        var testPutValue = "Test - Put";

        var efTestResult = new EntityCrudTestResult<GetPersonViewModel>();
        var mongoTestResult = new EntityCrudTestResult<GetPersonViewModel>();
        var exceptionMessage = string.Empty;

        try
        {
            efTestResult = await TestEfCrudAsync(
                requestModel.EfPersonId,
                testEfCreate,
                person => person.LastName, testPutValue,
                person => person.FirstName, testPatchValue,
                cancellationToken);

            mongoTestResult = await TestMongoCrudAsync(
                requestModel.MongoPersonId,
                testMongoCreate,
                person => person.LastName, testPutValue,
                person => person.FirstName, testPatchValue,
                cancellationToken);

            await transactionAction.Invoke(cancellationToken);
        }
        catch (Exception ex)
        {
            exceptionMessage = ex.Message;
            await _transactionManager.RollbackAsync(cancellationToken);
        }

        var efReadAgain = await _efPersonRead.GetByKeyAsync(efTestResult.Read.Id, cancellationToken);
        var efReadCreatedAgain = await _efPersonRead.GetByKeyAsync(efTestResult.Created.Id, cancellationToken);
        var mongoReadAgain = await _mongoPersonRead.GetByKeyAsync(mongoTestResult.Read.Id, cancellationToken);
        var mongoReadCreatedAgain = await _mongoPersonRead.GetByKeyAsync(mongoTestResult.Created.Id, cancellationToken);

        return Ok(new SessionTransactionAssertionViewModel
        {
            Ef = efTestResult with
            {
                PutWasCommitted = efReadAgain.LastName == testPutValue,
                PatchWasCommitted = efReadAgain.FirstName == testPatchValue,
                DeleteWasCommitted = efReadAgain.IsDeleted,
                CreateWasCommitted = efReadCreatedAgain != null,
                Created = efReadCreatedAgain?.ToViewModel(),
                Read = efReadAgain.ToViewModel()
            },
            Mongo = mongoTestResult with
            {
                PutWasCommitted = mongoReadAgain.LastName == testPutValue,
                PatchWasCommitted = mongoReadAgain.FirstName == testPatchValue,
                DeleteWasCommitted = mongoReadAgain.IsDeleted,
                CreateWasCommitted = mongoReadCreatedAgain != null,
                Created = mongoReadCreatedAgain?.ToViewModel(),
                Read = mongoReadAgain.ToViewModel()
            },
            ExceptionMessage = exceptionMessage
        });
    }

    private async Task<EntityCrudTestResult<GetPersonViewModel>> TestEfCrudAsync<TPatchProp, TPutProp>(
        Guid personId,
        EfPerson efPerson,
        Expression<Func<EfPerson, TPutProp>> putPath, TPutProp putValue,
        Expression<Func<EfPerson, TPatchProp>> patchPath, TPatchProp patchValue, CancellationToken cancellationToken)
    {
        var read = await _efPersonRead.GetByKeyAsync(personId, cancellationToken);
        read.ThrowExceptionIfNull("Failed to read entity in transaction!");
        var created = await _efPersonCreate.CreateAsync(efPerson, cancellationToken);
        created.ThrowExceptionIfNull("Failed to create entity in transaction!");
        var createdRead = await _efPersonRead.GetByKeyAsync(created.Id, cancellationToken);
        createdRead.ThrowExceptionIfNull("Failed to read created entity in transaction!");
        var patchDocEf = new JsonPatchDocument<EfPerson>();
        patchDocEf.Replace(patchPath, patchValue);
        var patched = await _efPersonUpdate.PatchAsync(read.Id, patchDocEf, cancellationToken);
        patched.ThrowExceptionIfNull("Failed to patch entity in transaction!");
        var tempEf = patched.Clone();
        tempEf.SetPropertyValue(putPath, putValue);
        var updated = await _efPersonUpdate.UpdateAsync(tempEf, cancellationToken);
        updated.ThrowExceptionIfNull("Failed to update entity in transaction!");
        var readAgain = await _efPersonRead.GetByKeyAsync(read.Id, cancellationToken);
        readAgain.ThrowExceptionIfNull("Failed to read updated entity in transaction!");
        var deleted = await _efPersonDelete.DeleteAsync(read.Id, cancellationToken);
        deleted.ThrowExceptionIfNull("Failed to delete entity in transaction!");

        return new EntityCrudTestResult<GetPersonViewModel>
        {
            Read = read.ToViewModel(),
            Created = created.ToViewModel(),
            ChangesCanBeReadInTransaction = readAgain.PropertyEquals(putPath, putValue) &&
                                            readAgain.PropertyEquals(patchPath, patchValue) &&
                                            createdRead.Id != Guid.Empty
        };
    }

    private async Task<EntityCrudTestResult<GetPersonViewModel>> TestMongoCrudAsync<TPatchProp, TPutProp>(
        Guid personId,
        MongoTenantPerson efPerson,
        Expression<Func<MongoTenantPerson, TPutProp>> putPath, TPutProp putValue,
        Expression<Func<MongoTenantPerson, TPatchProp>> patchPath, TPatchProp patchValue,
        CancellationToken cancellationToken)
    {
        var read = await _mongoPersonRead.GetByKeyAsync(personId, cancellationToken);
        read.ThrowExceptionIfNull("Failed to read entity in transaction!");
        var created = await _mongoPersonCreate.CreateAsync(efPerson, cancellationToken);
        created.ThrowExceptionIfNull("Failed to create entity in transaction!");
        var createdRead = await _mongoPersonRead.GetByKeyAsync(created.Id, cancellationToken);
        createdRead.ThrowExceptionIfNull("Failed to read created entity in transaction!");
        var patchDocEf = new JsonPatchDocument<MongoTenantPerson>();
        patchDocEf.Replace(patchPath, patchValue);
        var patched = await _mongoPersonUpdate.PatchAsync(read.Id, patchDocEf, cancellationToken);
        patched.ThrowExceptionIfNull("Failed to patch entity in transaction!");
        var tempEf = patched.Clone();
        tempEf.SetPropertyValue(putPath, putValue);
        var updated = await _mongoPersonUpdate.UpdateAsync(tempEf, cancellationToken);
        updated.ThrowExceptionIfNull("Failed to update entity in transaction!");
        var readAgain = await _mongoPersonRead.GetByKeyAsync(read.Id, cancellationToken);
        readAgain.ThrowExceptionIfNull("Failed to read updated entity in transaction!");
        var deleted = await _mongoPersonDelete.DeleteAsync(read.Id, cancellationToken);
        deleted.ThrowExceptionIfNull("Failed to delete entity in transaction!");

        return new EntityCrudTestResult<GetPersonViewModel>
        {
            Read = read.ToViewModel(),
            Created = created.ToViewModel(),
            ChangesCanBeReadInTransaction = readAgain.PropertyEquals(putPath, putValue) &&
                                            readAgain.PropertyEquals(patchPath, patchValue) &&
                                            createdRead.Id != Guid.Empty
        };
    }
}
