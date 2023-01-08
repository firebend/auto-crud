using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Firebend.AutoCrud.Mongo.Abstractions.Client;
using Firebend.AutoCrud.Mongo.Abstractions.Client.Indexing;
using Firebend.AutoCrud.Mongo.Configuration;
using Firebend.AutoCrud.Mongo.Helpers;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Mongo.Abstractions.Client.Indexing
{
    [TestFixture]
    public class MongoIndexMergeServiceTests
    {
        public class FakeAsyncCursor<T> : IAsyncCursor<T>
        {
            public FakeAsyncCursor(IEnumerable<T> enumerable)
            {
                Current = enumerable;
                Enumerator = Current.GetEnumerator();
            }

            public void Dispose() { }

            public bool MoveNext(CancellationToken cancellationToken = default) => Enumerator.MoveNext();

            public Task<bool> MoveNextAsync(CancellationToken cancellationToken = default) => Task.FromResult(Enumerator.MoveNext());

            public IEnumerable<T> Current { get; }

            public IEnumerator<T> Enumerator { get; }
        }

        public IFixture Fixture { get; set; }
        public Mock<IMongoCollection<FooIndexEntity>> Collection { get; set; }

        [SetUp]
        public void Setup()
        {
            new MongoDbConfigurator().Configure();

            Fixture = new Fixture().Customize(new AutoMoqCustomization());

            Fixture.Inject<IMongoRetryService>(new MongoRetryService());

            var doc = BsonDocument.Parse(@"{
        ""v"" : 2,
            ""key"" : {
                ""createdDate"" : 1,
                ""modifiedDate"" : -1
            },
            ""name"" : ""modified"",
            ""ns"" : ""fakeDb.fakeCollection""
        }");

            Collection = Fixture.Freeze<Mock<IMongoCollection<FooIndexEntity>>>();
            Collection.Setup(x => x.DocumentSerializer)
                .Returns(new BsonClassMapSerializer<FooIndexEntity>(BsonClassMap.LookupClassMap(typeof(FooIndexEntity))));

            Collection.Setup(x => x.Indexes.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FakeAsyncCursor<BsonDocument>(new[]
                {
                    doc
                }));

            Collection.Setup(x => x.Indexes.DropOneAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Collection.Setup(x => x.Indexes.CreateManyAsync(It.IsAny<IEnumerable<CreateIndexModel<FooIndexEntity>>>(), It.IsAny<CancellationToken>()))
                .Returns<IEnumerable<CreateIndexModel<FooIndexEntity>>, CancellationToken>((models, _) => Task.FromResult(models.Select(x => x.Options.Name)));
        }

        [TestCase]
        public async Task Mongo_Index_Merge_Service_Should_Create_Indexes()
        {
            //arrange
            var compareService = Fixture.Freeze<Mock<IMongoIndexComparisonService>>();
            compareService.Setup(x =>
                    x.DoesIndexMatch(It.IsAny<IMongoCollection<FooIndexEntity>>(), It.IsAny<BsonDocument>(), It.IsAny<CreateIndexModel<FooIndexEntity>>()))
                .Returns(false);

            compareService.Setup(x =>
                    x.EnsureUnique(It.IsAny<IMongoCollection<FooIndexEntity>>(), It.IsAny<CreateIndexModel<FooIndexEntity>[]>()))
                .Returns(true);

            var sut = Fixture.Create<MongoIndexMergeService<Guid, FooIndexEntity>>();
            var keys = Builders<FooIndexEntity>.IndexKeys;
            var date = MongoIndexProviderHelpers.DateTimeOffset(keys);
            date.Options.Name = "fake";

            //act
            await sut.MergeIndexesAsync(Collection.Object, new[] { date }, default);

            //assert
            Collection.Verify(x => x.Indexes.CreateManyAsync(It.IsAny<IEnumerable<CreateIndexModel<FooIndexEntity>>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase]
        public async Task Mongo_Index_Merge_Service_Should_Drop_And_Create_Indexes()
        {
            //arrange
            var compareService = Fixture.Freeze<Mock<IMongoIndexComparisonService>>();
            compareService.Setup(x =>
                    x.DoesIndexMatch(It.IsAny<IMongoCollection<FooIndexEntity>>(), It.IsAny<BsonDocument>(), It.IsAny<CreateIndexModel<FooIndexEntity>>()))
                .Returns(false);

            compareService.Setup(x =>
                    x.EnsureUnique(It.IsAny<IMongoCollection<FooIndexEntity>>(), It.IsAny<CreateIndexModel<FooIndexEntity>[]>()))
                .Returns(true);

            var sut = Fixture.Create<MongoIndexMergeService<Guid, FooIndexEntity>>();
            var keys = Builders<FooIndexEntity>.IndexKeys;
            var date = MongoIndexProviderHelpers.DateTimeOffset(keys);

            //act
            await sut.MergeIndexesAsync(Collection.Object, new[] { date }, default);

            //assert
            Collection.Verify(x => x.Indexes.CreateManyAsync(It.IsAny<IEnumerable<CreateIndexModel<FooIndexEntity>>>(), It.IsAny<CancellationToken>()), Times.Once);
            Collection.Verify(x => x.Indexes.DropOneAsync(It.Is<string>(str => str == date.Options.Name), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase]
        public async Task Mongo_Index_Merge_Service_Should_Add_All_When_No_Indexes_Exist()
        {
            //arrange
            Collection.Setup(x => x.Indexes.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FakeAsyncCursor<BsonDocument>(ArraySegment<BsonDocument>.Empty));

            var compareService = Fixture.Freeze<Mock<IMongoIndexComparisonService>>();

            compareService.Setup(x =>
                    x.EnsureUnique(It.IsAny<IMongoCollection<FooIndexEntity>>(), It.IsAny<CreateIndexModel<FooIndexEntity>[]>()))
                .Returns(true);

            var sut = Fixture.Create<MongoIndexMergeService<Guid, FooIndexEntity>>();
            var keys = Builders<FooIndexEntity>.IndexKeys;
            var date = MongoIndexProviderHelpers.DateTimeOffset(keys);

            //act
            await sut.MergeIndexesAsync(Collection.Object, new[] { date }, default);

            //assert
            Collection.Verify(x => x.Indexes.CreateManyAsync(It.IsAny<IEnumerable<CreateIndexModel<FooIndexEntity>>>(), It.IsAny<CancellationToken>()), Times.Once);
            Collection.Verify(x => x.Indexes.DropOneAsync(It.Is<string>(str => str == date.Options.Name), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestCase]
        public async Task Mongo_Index_Merge_Service_Should_Do_Nothing()
        {
            //arrange
            var compareService = Fixture.Freeze<Mock<IMongoIndexComparisonService>>();
            compareService.Setup(x =>
                    x.DoesIndexMatch(It.IsAny<IMongoCollection<FooIndexEntity>>(), It.IsAny<BsonDocument>(), It.IsAny<CreateIndexModel<FooIndexEntity>>()))
                .Returns(false);

            compareService.Setup(x =>
                    x.EnsureUnique(It.IsAny<IMongoCollection<FooIndexEntity>>(), It.IsAny<CreateIndexModel<FooIndexEntity>[]>()))
                .Returns(true);

            var sut = Fixture.Create<MongoIndexMergeService<Guid, FooIndexEntity>>();

            //act
            await sut.MergeIndexesAsync(Collection.Object, Array.Empty<CreateIndexModel<FooIndexEntity>>(), default);

            //assert
            Collection.Verify(x => x.Indexes.CreateManyAsync(It.IsAny<IEnumerable<CreateIndexModel<FooIndexEntity>>>(), It.IsAny<CancellationToken>()), Times.Never);
            Collection.Verify(x => x.Indexes.DropOneAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestCase]
        public async Task Mongo_Index_Merge_Service_Should_Add_New_Index()
        {
            //arrange
            var compareService = Fixture.Freeze<Mock<IMongoIndexComparisonService>>();
            compareService.Setup(x =>
                    x.DoesIndexMatch(It.IsAny<IMongoCollection<FooIndexEntity>>(), It.IsAny<BsonDocument>(), It.IsAny<CreateIndexModel<FooIndexEntity>>()))
                .Returns(true);

            compareService.Setup(x =>
                    x.EnsureUnique(It.IsAny<IMongoCollection<FooIndexEntity>>(), It.IsAny<CreateIndexModel<FooIndexEntity>[]>()))
                .Returns(true);

            var sut = Fixture.Create<MongoIndexMergeService<Guid, FooIndexEntity>>();
            var keys = Builders<FooIndexEntity>.IndexKeys;
            var date = MongoIndexProviderHelpers.DateTimeOffset(keys);
            var fullText = MongoIndexProviderHelpers.FullText(keys);

            //act
            await sut.MergeIndexesAsync(Collection.Object, new[] { date, fullText }, default);

            //assert
            Collection.Verify(x => x.Indexes.CreateManyAsync(It.Is<IEnumerable<CreateIndexModel<FooIndexEntity>>>(e => e.Count() == 1), It.IsAny<CancellationToken>()), Times.Once);
            Collection.Verify(x => x.Indexes.DropOneAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestCase]
        public async Task Mongo_Index_Merge_Service_Should_Add_New_Index_And_Drop_And_Recreate_Another()
        {
            //arrange
            var compareService = Fixture.Freeze<Mock<IMongoIndexComparisonService>>();
            compareService.Setup(x =>
                    x.DoesIndexMatch(It.IsAny<IMongoCollection<FooIndexEntity>>(), It.IsAny<BsonDocument>(), It.IsAny<CreateIndexModel<FooIndexEntity>>()))
                .Returns(false);

            compareService.Setup(x =>
                    x.EnsureUnique(It.IsAny<IMongoCollection<FooIndexEntity>>(), It.IsAny<CreateIndexModel<FooIndexEntity>[]>()))
                .Returns(true);

            var sut = Fixture.Create<MongoIndexMergeService<Guid, FooIndexEntity>>();
            var keys = Builders<FooIndexEntity>.IndexKeys;
            var date = MongoIndexProviderHelpers.DateTimeOffset(keys);
            var fullText = MongoIndexProviderHelpers.FullText(keys);

            //act
            await sut.MergeIndexesAsync(Collection.Object, new[] { date, fullText }, default);

            //assert
            Collection.Verify(x => x.Indexes.CreateManyAsync(It.Is<IEnumerable<CreateIndexModel<FooIndexEntity>>>(e => e.Count() == 2), It.IsAny<CancellationToken>()), Times.Once);
            Collection.Verify(x => x.Indexes.DropOneAsync(It.Is<string>(e => e == date.Options.Name), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
