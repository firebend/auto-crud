using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Caching;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.EntityFramework.Services;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Mongo.Services;
using Moq;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Caching;

[TestFixture]
public class TransactionalCacheInvalidationTests
{
    [Test]
    public async Task EntityFrameworkEntityUpdateService_Should_AddCacheInvalidationToOutbox_When_TransactionExists()
    {
        var entity = new TestEntity { Id = 123 };
        var updateClient = new Mock<IEntityFrameworkUpdateClient<int, TestEntity>>();
        updateClient.Setup(x => x.UpdateAsync(entity, It.IsAny<IEntityTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var transactionManager = new Mock<ISessionTransactionManager>();
        var (transaction, outbox) = CreateTransaction();
        var cacheService = new Mock<IEntityCacheService<int, TestEntity>>();
        var sut = new EntityFrameworkEntityUpdateService<int, TestEntity>(updateClient.Object,
            transactionManager.Object,
            cacheService.Object);

        await sut.UpdateAsync(entity, transaction.Object, default);

        cacheService.Verify(x => x.RemoveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        outbox.Verify(x => x.AddEnrollmentAsync(
                It.Is<EntityTransactionOutboxEnrollment>(e => e.TransactionId == transaction.Object.Id.ToString()
                    && e.EntityType == typeof(TestEntity)
                    && e.Enrollment is FunctionTransactionOutboxEnrollment),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task EntityFrameworkEntityUpdateService_Should_RemoveCacheImmediately_When_TransactionDoesNotExist()
    {
        var entity = new TestEntity { Id = 123 };
        var updateClient = new Mock<IEntityFrameworkUpdateClient<int, TestEntity>>();
        updateClient.Setup(x => x.UpdateAsync(entity, It.IsAny<IEntityTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var transactionManager = new Mock<ISessionTransactionManager>();
        transactionManager.Setup(x => x.GetTransaction<int, TestEntity>(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEntityTransaction)null);
        var cacheService = new Mock<IEntityCacheService<int, TestEntity>>();
        var sut = new EntityFrameworkEntityUpdateService<int, TestEntity>(updateClient.Object,
            transactionManager.Object,
            cacheService.Object);

        await sut.UpdateAsync(entity, default);

        cacheService.Verify(x => x.RemoveAsync(entity.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task EntityFrameworkEntityDeleteService_Should_AddCacheInvalidationToOutbox_When_TransactionExists()
    {
        var entity = new TestEntity { Id = 123 };
        var deleteClient = new Mock<IEntityFrameworkDeleteClient<int, TestEntity>>();
        deleteClient.Setup(x => x.DeleteAsync(entity.Id, It.IsAny<IEntityTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var transactionManager = new Mock<ISessionTransactionManager>();
        var (transaction, outbox) = CreateTransaction();
        var cacheService = new Mock<IEntityCacheService<int, TestEntity>>();
        var sut = new EntityFrameworkEntityDeleteService<int, TestEntity>(deleteClient.Object,
            transactionManager.Object,
            cacheService.Object);

        await sut.DeleteAsync(entity.Id, transaction.Object, default);

        cacheService.Verify(x => x.RemoveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        outbox.Verify(x => x.AddEnrollmentAsync(
                It.Is<EntityTransactionOutboxEnrollment>(e => e.TransactionId == transaction.Object.Id.ToString()
                    && e.EntityType == typeof(TestEntity)
                    && e.Enrollment is FunctionTransactionOutboxEnrollment),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task EntityFrameworkEntityDeleteService_Should_RemoveCacheImmediately_When_TransactionDoesNotExist()
    {
        var entity = new TestEntity { Id = 123 };
        var deleteClient = new Mock<IEntityFrameworkDeleteClient<int, TestEntity>>();
        deleteClient.Setup(x => x.DeleteAsync(entity.Id, It.IsAny<IEntityTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var transactionManager = new Mock<ISessionTransactionManager>();
        transactionManager.Setup(x => x.GetTransaction<int, TestEntity>(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEntityTransaction)null);
        var cacheService = new Mock<IEntityCacheService<int, TestEntity>>();
        var sut = new EntityFrameworkEntityDeleteService<int, TestEntity>(deleteClient.Object,
            transactionManager.Object,
            cacheService.Object);

        await sut.DeleteAsync(entity.Id, default);

        cacheService.Verify(x => x.RemoveAsync(entity.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task EntityFrameworkEntitySoftDeleteService_Should_DelegateCacheInvalidationToUpdateService()
    {
        var entity = new SoftDeleteTestEntity { Id = 123 };
        var updateService = new Mock<IEntityUpdateService<int, SoftDeleteTestEntity>>();
        var transactionManager = new Mock<ISessionTransactionManager>();
        var (transaction, outbox) = CreateTransaction();
        updateService.Setup(x => x.PatchAsync(entity.Id,
                It.IsAny<Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<SoftDeleteTestEntity>>(),
                transaction.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var sut = new EntityFrameworkEntitySoftDeleteService<int, SoftDeleteTestEntity>(updateService.Object,
            transactionManager.Object);

        await sut.DeleteAsync(entity.Id, transaction.Object, default);

        updateService.Verify(x => x.PatchAsync(entity.Id,
                It.IsAny<Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<SoftDeleteTestEntity>>(),
                transaction.Object,
                It.IsAny<CancellationToken>()),
            Times.Once);
        outbox.Verify(x => x.AddEnrollmentAsync(It.IsAny<EntityTransactionOutboxEnrollment>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task MongoEntityUpdateService_Should_AddCacheInvalidationToOutbox_When_TransactionExists()
    {
        var entity = new TestEntity { Id = 123 };
        var updateClient = new Mock<IMongoUpdateClient<int, TestEntity>>();
        updateClient.Setup(x => x.UpsertAsync(entity, It.IsAny<IEntityTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var transactionManager = new Mock<ISessionTransactionManager>();
        var (transaction, outbox) = CreateTransaction();
        var cacheService = new Mock<IEntityCacheService<int, TestEntity>>();
        var sut = new MongoEntityUpdateService<int, TestEntity>(updateClient.Object,
            transactionManager.Object,
            cacheService.Object);

        await sut.UpdateAsync(entity, transaction.Object, default);

        cacheService.Verify(x => x.RemoveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        outbox.Verify(x => x.AddEnrollmentAsync(
                It.Is<EntityTransactionOutboxEnrollment>(e => e.TransactionId == transaction.Object.Id.ToString()
                    && e.EntityType == typeof(TestEntity)
                    && e.Enrollment is FunctionTransactionOutboxEnrollment),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task MongoEntityUpdateService_Should_RemoveCacheImmediately_When_TransactionDoesNotExist()
    {
        var entity = new TestEntity { Id = 123 };
        var updateClient = new Mock<IMongoUpdateClient<int, TestEntity>>();
        updateClient.Setup(x => x.UpsertAsync(entity, It.IsAny<IEntityTransaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var transactionManager = new Mock<ISessionTransactionManager>();
        transactionManager.Setup(x => x.GetTransaction<int, TestEntity>(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEntityTransaction)null);
        var cacheService = new Mock<IEntityCacheService<int, TestEntity>>();
        var sut = new MongoEntityUpdateService<int, TestEntity>(updateClient.Object,
            transactionManager.Object,
            cacheService.Object);

        await sut.UpdateAsync(entity, default);

        cacheService.Verify(x => x.RemoveAsync(entity.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task MongoEntityDeleteService_Should_AddCacheInvalidationToOutbox_When_TransactionExists()
    {
        var entity = new TestEntity { Id = 123 };
        var deleteClient = new Mock<IMongoDeleteClient<int, TestEntity>>();
        deleteClient.Setup(x => x.DeleteAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(),
                It.IsAny<IEntityTransaction>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var transactionManager = new Mock<ISessionTransactionManager>();
        var (transaction, outbox) = CreateTransaction();
        var cacheService = new Mock<IEntityCacheService<int, TestEntity>>();
        var sut = new MongoEntityDeleteService<int, TestEntity>(deleteClient.Object,
            transactionManager.Object,
            cacheService.Object);

        await sut.DeleteAsync(entity.Id, transaction.Object, default);

        cacheService.Verify(x => x.RemoveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        outbox.Verify(x => x.AddEnrollmentAsync(
                It.Is<EntityTransactionOutboxEnrollment>(e => e.TransactionId == transaction.Object.Id.ToString()
                    && e.EntityType == typeof(TestEntity)
                    && e.Enrollment is FunctionTransactionOutboxEnrollment),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task MongoEntityDeleteService_Should_RemoveCacheImmediately_When_TransactionDoesNotExist()
    {
        var entity = new TestEntity { Id = 123 };
        var deleteClient = new Mock<IMongoDeleteClient<int, TestEntity>>();
        deleteClient.Setup(x => x.DeleteAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(),
                It.IsAny<IEntityTransaction>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var transactionManager = new Mock<ISessionTransactionManager>();
        transactionManager.Setup(x => x.GetTransaction<int, TestEntity>(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEntityTransaction)null);
        var cacheService = new Mock<IEntityCacheService<int, TestEntity>>();
        var sut = new MongoEntityDeleteService<int, TestEntity>(deleteClient.Object,
            transactionManager.Object,
            cacheService.Object);

        await sut.DeleteAsync(entity.Id, default);

        cacheService.Verify(x => x.RemoveAsync(entity.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task MongoEntitySoftDeleteService_Should_DelegateCacheInvalidationToUpdateService()
    {
        var entity = new SoftDeleteTestEntity { Id = 123 };
        var updateService = new Mock<IEntityUpdateService<int, SoftDeleteTestEntity>>();
        var transactionManager = new Mock<ISessionTransactionManager>();
        var (transaction, outbox) = CreateTransaction();
        updateService.Setup(x => x.PatchAsync(entity.Id,
                It.IsAny<Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<SoftDeleteTestEntity>>(),
                transaction.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var sut = new MongoEntitySoftDeleteService<int, SoftDeleteTestEntity>(updateService.Object,
            transactionManager.Object);

        await sut.DeleteAsync(entity.Id, transaction.Object, default);

        updateService.Verify(x => x.PatchAsync(entity.Id,
                It.IsAny<Microsoft.AspNetCore.JsonPatch.JsonPatchDocument<SoftDeleteTestEntity>>(),
                transaction.Object,
                It.IsAny<CancellationToken>()),
            Times.Once);
        outbox.Verify(x => x.AddEnrollmentAsync(It.IsAny<EntityTransactionOutboxEnrollment>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static (Mock<IEntityTransaction> Transaction, Mock<IEntityTransactionOutbox> Outbox) CreateTransaction()
    {
        var outbox = new Mock<IEntityTransactionOutbox>();
        var transaction = new Mock<IEntityTransaction>();
        transaction.SetupGet(x => x.Id).Returns(Guid.NewGuid());
        transaction.SetupGet(x => x.Outbox).Returns(outbox.Object);
        return (transaction, outbox);
    }

    public class SoftDeleteTestEntity : IEntity<int>, IActiveEntity
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
    }
}
