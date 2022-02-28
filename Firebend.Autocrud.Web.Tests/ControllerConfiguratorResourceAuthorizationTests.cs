using System;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firebend.Autocrud.Web.Tests;

[TestClass]
public class ControllerConfiguratorResourceAuthorizationTests
{
    [TestMethod]
    public async Task Add_Resource_Authorization_Should_Return_A_Controller_Configurator()
    {
        // arrange
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());

        var controllerConfigurator = fixture.Build<ControllerConfigurator<EntityCrudBuilder<Guid, TestEntity>,Guid,TestEntity>>().Create();

        // act
        var result = controllerConfigurator.AddResourceAuthorization();

        //assert
        result.GetType().Should()
            .Be(typeof(ControllerConfigurator<EntityCrudBuilder<Guid, TestEntity>, Guid, TestEntity>));
    }
}

public class TestEntity : IEntity<Guid>, IActiveEntity, IModifiedEntity, ITenantEntity<int>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Guid Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset ModifiedDate { get; set; }
    public int TenantId { get; set; }
}
