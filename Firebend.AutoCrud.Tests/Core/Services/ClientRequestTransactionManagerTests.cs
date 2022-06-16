using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Firebend.AutoCrud.Core.Implementations.Entities;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Core.Services;

[TestFixture]
public class ClientRequestTransactionManagerTests
{
    private IFixture _fixture;
    private Mock<IServiceProvider> _serviceProvider;
    private Mock<IEntityTransactionFactory<Guid, TestClassEf>> _efTransactionFactory;
    private Mock<IEntityTransactionFactory<Guid, TestClassMongo>> _mongoTransactionFactory;
    private List<Mock<TestTransaction>> _transactions;

    [SetUp]
    public void TestSetup()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
        _transactions = new List<Mock<TestTransaction>>();

        _efTransactionFactory = _fixture.Create<Mock<IEntityTransactionFactory<Guid, TestClassEf>>>();
        _mongoTransactionFactory = _fixture.Create<Mock<IEntityTransactionFactory<Guid, TestClassMongo>>>();
        _efTransactionFactory.Setup(x => x.StartTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => CreateMockTransaction().Object);
        _mongoTransactionFactory.Setup(x => x.StartTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => CreateMockTransaction().Object);

        _serviceProvider = _fixture.Freeze<Mock<IServiceProvider>>();
        _serviceProvider.Setup(x => x.GetService(typeof(IEntityTransactionFactory<Guid, TestClassEf>)))
            .Returns(_efTransactionFactory.Object);
        _serviceProvider.Setup(x => x.GetService(typeof(IEntityTransactionFactory<Guid, TestClassMongo>)))
            .Returns(_mongoTransactionFactory.Object);
    }

    private Mock<TestTransaction> CreateMockTransaction()
    {
        var transaction = _fixture.Create<Mock<TestTransaction>>();
        _transactions.Add(transaction);
        return transaction;
    }

    [Test]
    public async Task GetTransaction_Should_OnlyCreateOneInstancePerTransactionFactoryType()
    {
        // arrange
        var sut = _fixture.Create<ClientRequestTransactionManager>();
        sut.Start();

        // act
        var efCall1 = await sut.GetTransaction<Guid, TestClassEf>(default);
        var efCall2 = await sut.GetTransaction<Guid, TestClassEf>(default);
        var mongoCall1 = await sut.GetTransaction<Guid, TestClassMongo>(default);
        var mongoCall2 = await sut.GetTransaction<Guid, TestClassMongo>(default);

        // assert
        sut.TransactionStarted.Should().BeTrue();
        efCall1.Should().NotBeNull();
        efCall2.Should().NotBeNull();
        efCall1.Should().BeSameAs(efCall2);

        mongoCall1.Should().NotBeNull();
        mongoCall2.Should().NotBeNull();
        mongoCall1.Should().BeSameAs(mongoCall2);
        _transactions.Should().HaveCount(2);
    }

    [Test]
    public async Task GetTransaction_Should_TrackTransactionsByFactoryType()
    {
        // arrange
        var sut = _fixture.Create<ClientRequestTransactionManager>();
        sut.Start();

        // act
        var efTransaction = await sut.GetTransaction<Guid, TestClassEf>(default);
        var mongoTransaction = await sut.GetTransaction<Guid, TestClassMongo>(default);

        // assert
        sut.TransactionStarted.Should().BeTrue();
        efTransaction.Should().NotBeNull();
        mongoTransaction.Should().NotBeNull();
        efTransaction.Should().NotBeSameAs(mongoTransaction);
        sut.TransactionIds.Should().HaveCount(2);
    }

    [Test]
    public async Task GetTransaction_Should_ReturnNull_When_SessionTransactionIsNotStarted()
    {
        // arrange
        var sut = _fixture.Create<ClientRequestTransactionManager>();

        // act
        var efTransaction = await sut.GetTransaction<Guid, TestClassEf>(default);
        var mongoTransaction = await sut.GetTransaction<Guid, TestClassMongo>(default);

        // assert
        sut.TransactionStarted.Should().BeFalse();
        efTransaction.Should().BeNull();
        mongoTransaction.Should().BeNull();
        sut.TransactionIds.Should().HaveCount(0);
    }

    [Test]
    public async Task Dispose_Should_DisposeAllTransactions()
    {
        // arrange
        var transactionToAdd = CreateMockTransaction();
        var sut = _fixture.Create<ClientRequestTransactionManager>();
        sut.Start();
        var efTransaction = await sut.GetTransaction<Guid, TestClassEf>(default);
        var mongoTransaction = await sut.GetTransaction<Guid, TestClassMongo>(default);
        efTransaction.Should().NotBeNull();
        mongoTransaction.Should().NotBeNull();
        sut.AddTransaction(transactionToAdd.Object);
        sut.TransactionIds.Should().HaveCount(3);

        // act
        sut.Dispose();

        // assert
        _transactions[0].Verify(x => x.Dispose(), Times.Once);
        _transactions[1].Verify(x => x.Dispose(), Times.Once);
        _transactions[2].Verify(x => x.Dispose(), Times.Once);
    }

    [Test]
    public async Task AddTransaction_Should_DoNothing_If_SessionTransactionStarted_And_TransactionAlreadyBeingTracked()
    {
        // arrange
        var sut = _fixture.Create<ClientRequestTransactionManager>();
        sut.Start();
        var efTransaction = await sut.GetTransaction<Guid, TestClassEf>(default);
        efTransaction.Should().NotBeNull();
        efTransaction.Id.Should().NotBeEmpty();

        // act
        sut.AddTransaction(efTransaction);

        // assert
        sut.TransactionIds.Should().HaveCount(1);
    }

    [Test]
    public void AddTransaction_Should_DoNothing_If_SessionTransactionNotStarted()
    {
        // arrange
        var transactionToAdd = CreateMockTransaction();
        var sut = _fixture.Create<ClientRequestTransactionManager>();

        // act
        sut.AddTransaction(transactionToAdd.Object);

        // assert
        sut.TransactionIds.Should().HaveCount(0);
    }

    [Test]
    public void AddTransaction_Should_DoNothing_If_TransactionIsNull()
    {
        // arrange
        var sut = _fixture.Create<ClientRequestTransactionManager>();

        // act
        sut.AddTransaction(null);

        // assert
        sut.TransactionIds.Should().HaveCount(0);
    }

    [Test]
    public async Task AddTransaction_Should_AddTransactionIfStartedValidAndNotBeingTracked()
    {
        // arrange
        var transactionToAdd = CreateMockTransaction();
        var sut = _fixture.Create<ClientRequestTransactionManager>();
        sut.Start();
        var efTransaction = await sut.GetTransaction<Guid, TestClassEf>(default);
        var mongoTransaction = await sut.GetTransaction<Guid, TestClassMongo>(default);
        efTransaction.Should().NotBeNull();
        mongoTransaction.Should().NotBeNull();

        // act
        sut.AddTransaction(transactionToAdd.Object);

        // assert
        sut.TransactionIds.Should().HaveCount(3);
    }

    [Test]
    public async Task CompleteAsync_Should_CallCompleteAsyncOnEachTransactionAndDisposeTransaction()
    {
        // arrange
        var sut = _fixture.Create<ClientRequestTransactionManager>();
        sut.Start();
        var efTransaction = await sut.GetTransaction<Guid, TestClassEf>(default);
        var mongoTransaction = await sut.GetTransaction<Guid, TestClassMongo>(default);
        efTransaction.Should().NotBeNull();
        mongoTransaction.Should().NotBeNull();
        sut.AddTransaction(CreateMockTransaction().Object);
        sut.TransactionIds.Should().HaveCount(3);

        // act
        await sut.CompleteAsync(default);

        // assert
        foreach (var transaction in _transactions)
        {
            transaction.Verify(x => x.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
            transaction.Verify(x => x.Dispose(), Times.Once);
        }
    }

    [Test]
    public async Task RollbackAsync_Should_CallRollbackAsyncOnEachTransactionAndDisposeTransaction()
    {
        // arrange
        var sut = _fixture.Create<ClientRequestTransactionManager>();
        sut.Start();
        var efTransaction = await sut.GetTransaction<Guid, TestClassEf>(default);
        var mongoTransaction = await sut.GetTransaction<Guid, TestClassMongo>(default);
        efTransaction.Should().NotBeNull();
        mongoTransaction.Should().NotBeNull();
        sut.AddTransaction(CreateMockTransaction().Object);
        sut.TransactionIds.Should().HaveCount(3);

        // act
        await sut.RollbackAsync(default);

        // assert
        foreach (var transaction in _transactions)
        {
            transaction.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
            transaction.Verify(x => x.Dispose(), Times.Once);
        }
    }

    public class TestClassEf : IEntity<Guid>
    {
        public Guid Id { get; set; }
    }

    public class TestClassMongo : IEntity<Guid>
    {
        public Guid Id { get; set; }
    }

    public abstract class TestTransaction : IEntityTransaction
    {
        public abstract void Dispose();
        public Guid Id { get; } = Guid.NewGuid();
        public abstract Task CompleteAsync(CancellationToken cancellationToken);
        public abstract Task RollbackAsync(CancellationToken cancellationToken);
        public IEntityTransactionOutbox Outbox => null;
    }
}
