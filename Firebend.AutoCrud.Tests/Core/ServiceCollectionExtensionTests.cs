using Firebend.AutoCrud.Core.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Core;

[TestFixture]
public class ServiceCollectionExtensionTests
{
    [TestCase]
    public void Register_All_Types_Should_Register_Services_By_Given_Interfaces()
    {
        // given
        var serviceCollection = new ServiceCollection();

        // when
        serviceCollection.RegisterAllTypes<ITestService>(new[] { typeof(ServiceCollectionExtensionTests).Assembly });
        var serviceBuilder = serviceCollection.BuildServiceProvider();

        var testService = serviceBuilder.GetService<ITestService>();

        // then
        testService.Should().NotBeNull();
    }
}

public interface ITestService;

public class TestServiceA : ITestService;

public class TestServiceB : ITestService;
