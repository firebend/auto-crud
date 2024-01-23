using System;
using System.Threading.Tasks;
using AutoFixture;
using Firebend.AutoCrud.Mongo.Abstractions.Client;
using FluentAssertions;
using Microsoft.AspNetCore.Connections;
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
        Task<bool> FakeFunc()
        {
            count++;
            return Task.FromResult(true);
        }

        //act
        var result = await retryService.RetryErrorAsync(FakeFunc, 10);

        //assert
        result.Should().BeTrue();
        count.Should().Be(1);
    }

    [TestCase]
    public async Task Mongo_Retry_Service_Should_Retry()
    {
        //arrange
        var fixture = new Fixture();

        var retryService = fixture.Create<MongoRetryService>();

        var count = 0;
        Task<bool> FakeFunc()
        {
            count++;

            if (count < 3)
            {
                var connId = new ConnectionId(new ServerId(new ClusterId(), new UriEndPoint(new Uri("https://www.google.com"))));
                throw new MongoWriteException(connId, null, null, null);
            }

            return Task.FromResult(true);
        }

        //act
        var result = await retryService.RetryErrorAsync(FakeFunc, 10);

        //assert
        result.Should().BeTrue();
        count.Should().Be(3);
    }

    [TestCase]
    public async Task Mongo_Retry_Service_Should_Not_Retry_If_Invalid_Exception()
    {
        //arrange
        var fixture = new Fixture();

        var retryService = fixture.Create<MongoRetryService>();

        var count = 0;
        Task<bool> FakeFunc()
        {
            count++;

            if (count < 3)
            {
                throw new Exception("died");
            }

            return Task.FromResult(true);
        }


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
    }

    [TestCase]
    public async Task Mongo_Retry_Service_Should_Retry_Maximum_Number_Of_Times()
    {
        //arrange
        var fixture = new Fixture();

        var retryService = fixture.Create<MongoRetryService>();

        var count = 0;
        Task<bool> FakeFunc()
        {
            count++;

            var connId = new ConnectionId(new ServerId(new ClusterId(), new UriEndPoint(new Uri("https://www.google.com"))));
            throw new MongoWriteException(connId, null, null, null);
        }

        //act
        try
        {
            await retryService.RetryErrorAsync(FakeFunc, 4);
        }
        catch (Exception ex)
        {
            ex.Message.Should().Contain("A write operation resulted in an error.");
        }

        //assert
        count.Should().Be(4);
    }
}
