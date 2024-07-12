using System;
using System.Threading.Tasks;
using AutoFixture;
using Firebend.AutoCrud.Mongo.Client;
using FluentAssertions;
using Microsoft.AspNetCore.Connections;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Mongo;

[TestFixture]
public class MongoRetryServiceTests
{
    [TestCase]
    public async Task Mongo_Retry_Service_Should_Only_Try_Once()
    {
        //arrange
        var fixture = new Fixture();

        var retryService = fixture.Create<MongoRetryService>();

        var count = 0;

        //act
        var result = await retryService.RetryErrorAsync(FakeFunc, 10);

        //assert
        result.Should().BeTrue();
        count.Should().Be(1);
        return;

        Task<bool> FakeFunc()
        {
            count++;
            return Task.FromResult(true);
        }
    }

    [TestCase]
    public async Task Mongo_Retry_Service_Should_Retry()
    {
        //arrange
        var fixture = new Fixture();

        var retryService = fixture.Create<MongoRetryService>();

        var count = 0;

        //act
        var result = await retryService.RetryErrorAsync(FakeFunc, 10);

        //assert
        result.Should().BeTrue();
        count.Should().Be(3);
        return;

        Task<bool> FakeFunc()
        {
            count++;

            if (count >= 3)
            {
                return Task.FromResult(true);
            }

            throw GetMongoCommandError();
        }
    }

    [TestCase]
    public async Task Mongo_Retry_Service_Should_Not_Retry_If_Invalid_Exception()
    {
        //arrange
        var fixture = new Fixture();

        var retryService = fixture.Create<MongoRetryService>();

        var count = 0;


        //act
        var result = false;
        Exception ex = null;
        try
        {
            result = await retryService.RetryErrorAsync(FakeFunc, 10);
        }
        catch (Exception e)
        {
            ex = e;
        }

        //assert
        result.Should().BeFalse();
        count.Should().Be(1);
        ex?.Message.Should().Be("died");
        return;

        Task<bool> FakeFunc()
        {
            count++;

            if (count < 3)
            {
                throw new Exception("died");
            }

            return Task.FromResult(true);
        }
    }

    [TestCase]
    public async Task Mongo_Retry_Service_Should_Retry_Maximum_Number_Of_Times()
    {
        //arrange
        var fixture = new Fixture();

        var retryService = fixture.Create<MongoRetryService>();

        var count = 0;

        //act
        try
        {
            await retryService.RetryErrorAsync(FakeFunc, 4);
        }
        catch
        {
            //ignore
        }

        //assert
        count.Should().Be(4);
        return;

        Task<bool> FakeFunc()
        {
            count++;

            throw GetMongoCommandError();
        }
    }

    private static MongoCommandException GetMongoCommandError()
    {
        var connId = new ConnectionId(new ServerId(new ClusterId(), new UriEndPoint(new Uri("https://www.google.com"))));
        return new MongoCommandException(connId, null, null, BsonDocument.Parse("{ code: 42 }"));
    }
}
