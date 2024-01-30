using AutoFixture;
using AutoFixture.AutoMoq;
using Firebend.AutoCrud.Mongo.Client.Indexing;
using Firebend.AutoCrud.Mongo.Configuration;
using Firebend.AutoCrud.Mongo.Helpers;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Mongo.Abstractions.Client.Indexing;

[TestFixture]
public class MongoIndexComparisonServiceTests
{
    [SetUp]
    public void Setup()
    {
        new MongoDbConfigurator().Configure();
    }

    [TestCase]
    public void Mongo_Index_Comparison_Service_Should_Pass()
    {
        //arrange
        var keys = Builders<FooIndexEntity>.IndexKeys;
        var text = MongoIndexProviderHelpers.FullText(keys);
        var modified = MongoIndexProviderHelpers.DateTimeOffset(keys);

        var indexes = new[]
        {
            text,
            modified
        };

        var fixture = new Fixture().Customize(new AutoMoqCustomization());

        var collection = fixture.Freeze<Mock<IMongoCollection<FooIndexEntity>>>();
        collection.Setup(x => x.DocumentSerializer)
            .Returns(new BsonClassMapSerializer<FooIndexEntity>(BsonClassMap.LookupClassMap(typeof(FooIndexEntity))));

        var sut = fixture.Create<MongoIndexComparisonService>();

        //act
        var result = sut.EnsureUnique(collection.Object, indexes);

        //assert
        result.Should().BeTrue();
    }

    [TestCase]
    public void Mongo_Index_Comparison_Service_Should_Find_Duplicate_Text_Indexes()
    {
        //arrange
        var keys = Builders<FooIndexEntity>.IndexKeys;
        var text1 = MongoIndexProviderHelpers.FullText(keys);
        var text2 = MongoIndexProviderHelpers.FullText(keys);

        var indexes = new[]
        {
            text1,
            text2
        };

        var fixture = new Fixture().Customize(new AutoMoqCustomization());

        var collection = fixture.Freeze<Mock<IMongoCollection<FooIndexEntity>>>();
        collection.Setup(x => x.DocumentSerializer)
            .Returns(new BsonClassMapSerializer<FooIndexEntity>(BsonClassMap.LookupClassMap(typeof(FooIndexEntity))));

        var sut = fixture.Create<MongoIndexComparisonService>();

        //act
        var result = sut.EnsureUnique(collection.Object, indexes);

        //assert
        result.Should().BeFalse();
    }

    [TestCase]
    public void Mongo_Index_Comparison_Service_Should_Find_Duplicate_Bson_Indexes()
    {
        //arrange
        var keys = Builders<FooIndexEntity>.IndexKeys;
        var date1 = MongoIndexProviderHelpers.DateTimeOffset(keys);
        var date2 = MongoIndexProviderHelpers.DateTimeOffset(keys);

        var indexes = new[]
        {
            date1,
            date2
        };

        var fixture = new Fixture().Customize(new AutoMoqCustomization());

        var collection = fixture.Freeze<Mock<IMongoCollection<FooIndexEntity>>>();
        collection.Setup(x => x.DocumentSerializer)
            .Returns(new BsonClassMapSerializer<FooIndexEntity>(BsonClassMap.LookupClassMap(typeof(FooIndexEntity))));

        var sut = fixture.Create<MongoIndexComparisonService>();

        //act
        var result = sut.EnsureUnique(collection.Object, indexes);

        //assert
        result.Should().BeFalse();
    }

    [TestCase]
    public void Mongo_Index_Comparison_Service_Should_Find_Matching_Date_Index()
    {
        //arrange
        var keys = Builders<FooIndexEntity>.IndexKeys;
        var date = MongoIndexProviderHelpers.DateTimeOffset(keys);
        var doc = BsonDocument.Parse(@"{
        ""v"" : 2,
            ""key"" : {
                ""createdDate"" : 1,
                ""modifiedDate"" : 1
            },
            ""name"" : ""modified"",
            ""ns"" : ""fakeDb.fakeCollection""
        }");

        var fixture = new Fixture().Customize(new AutoMoqCustomization());

        var collection = fixture.Freeze<Mock<IMongoCollection<FooIndexEntity>>>();
        collection.Setup(x => x.DocumentSerializer)
            .Returns(new BsonClassMapSerializer<FooIndexEntity>(BsonClassMap.LookupClassMap(typeof(FooIndexEntity))));

        var sut = fixture.Create<MongoIndexComparisonService>();

        //act
        var result = sut.DoesIndexMatch(collection.Object, doc, date);

        //assert
        result.Should().BeTrue();
    }

    [TestCase]
    public void Mongo_Index_Comparison_Service_Should_Not_Find_Matching_Date_Index()
    {
        //arrange
        var keys = Builders<FooIndexEntity>.IndexKeys;
        var date = MongoIndexProviderHelpers.DateTimeOffset(keys);
        var doc = BsonDocument.Parse(@"{
        ""v"" : 2,
            ""key"" : {
                ""createdDate"" : 1,
                ""modifiedDate"" : -1
            },
            ""name"" : ""modified"",
            ""ns"" : ""fakeDb.fakeCollection""
        }");

        var fixture = new Fixture().Customize(new AutoMoqCustomization());

        var collection = fixture.Freeze<Mock<IMongoCollection<FooIndexEntity>>>();
        collection.Setup(x => x.DocumentSerializer)
            .Returns(new BsonClassMapSerializer<FooIndexEntity>(BsonClassMap.LookupClassMap(typeof(FooIndexEntity))));

        var sut = fixture.Create<MongoIndexComparisonService>();

        //act
        var result = sut.DoesIndexMatch(collection.Object, doc, date);

        //assert
        result.Should().BeFalse();
    }

    [TestCase]
    public void Mongo_Index_Comparison_Service_Should_Find_Matching_Text_Index()
    {
        //arrange
        var keys = Builders<FooIndexEntity>.IndexKeys;
        var index = MongoIndexProviderHelpers.FullText(keys);
        var doc = BsonDocument.Parse(@"{
    ""v"" : 2,
    ""key"" : {
        ""_fts"" : ""text"",
        ""_ftsx"" : 1
    },
    ""name"" : ""text"",
    ""ns"" : ""fakeDb.fakeCollection"",
    ""weights"" : {
        ""$**"" : 1
    },
    ""default_language"" : ""english"",
    ""language_override"" : ""language"",
    ""textIndexVersion"" : 3
}");

        var fixture = new Fixture().Customize(new AutoMoqCustomization());

        var collection = fixture.Freeze<Mock<IMongoCollection<FooIndexEntity>>>();
        collection.Setup(x => x.DocumentSerializer)
            .Returns(new BsonClassMapSerializer<FooIndexEntity>(BsonClassMap.LookupClassMap(typeof(FooIndexEntity))));

        var sut = fixture.Create<MongoIndexComparisonService>();

        //act
        var result = sut.DoesIndexMatch(collection.Object, doc, index);

        //assert
        result.Should().BeTrue();
    }

    [TestCase]
    public void Mongo_Index_Comparison_Service_Should_Not_Find_Matching_Text_Index()
    {
        //arrange
        var keys = Builders<FooIndexEntity>.IndexKeys;
        var index = MongoIndexProviderHelpers.FullText(keys);
        var doc = BsonDocument.Parse(@"{
    ""v"" : 2,
    ""key"" : {
        ""_fts"" : ""text"",
        ""_ftsx"" : 1
    },
    ""name"" : ""text"",
    ""ns"" : ""fakeDb.fakeCollection"",
    ""weights"" : {
        ""text"" : 1
    },
    ""default_language"" : ""english"",
    ""language_override"" : ""language"",
    ""textIndexVersion"" : 3
}");

        var fixture = new Fixture().Customize(new AutoMoqCustomization());

        var collection = fixture.Freeze<Mock<IMongoCollection<FooIndexEntity>>>();
        collection.Setup(x => x.DocumentSerializer)
            .Returns(new BsonClassMapSerializer<FooIndexEntity>(BsonClassMap.LookupClassMap(typeof(FooIndexEntity))));

        var sut = fixture.Create<MongoIndexComparisonService>();

        //act
        var result = sut.DoesIndexMatch(collection.Object, doc, index);

        //assert
        result.Should().BeFalse();
    }
}
