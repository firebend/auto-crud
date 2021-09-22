using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Attributes;
using Firebend.AutoCrud.Web.Implementations.Swagger;
using Firebend.AutoCrud.Web.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NUnit.Framework;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Firebend.AutoCrud.Tests.Web.Implementations.Swagger
{
    public class FakeEntity : IEntity<Guid>
    {
        public Guid Id { get; set; }
    }

    public class FakeController : AbstractEntityReadController<Guid, FakeEntity, FakeEntity>
    {
        public FakeController(IEntityReadService<Guid, FakeEntity> readService,
            IEntityKeyParser<Guid, FakeEntity> entityKeyParser,
            IReadViewModelMapper<Guid, FakeEntity, FakeEntity> viewModelMapper,
            IOptions<ApiBehaviorOptions> apiOptions) : base(readService, entityKeyParser, viewModelMapper, apiOptions)
        {
        }
    }

    [TestFixture]
    public class SwaggerOperationFilterTests
    {
        [Test]
        public void Swagger_Operation_Filter_Should_Assign_Operation_Id_Using_Entity_Name_Attribute()
        {
            //arrange
            var openApiOperation = new OpenApiOperation();
            var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ReadByIdAsync));
            var generator = new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new JsonSerializerOptions()));
            var apiDescription = new ApiDescription
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    EndpointMetadata = new List<object> { new OpenApiEntityNameAttribute("Fake", "Fakes") },
                    ControllerTypeInfo = typeof(FakeController).GetTypeInfo(),
                    ActionName = nameof(FakeController.ReadByIdAsync).Replace("Async", null)
                }
            };
            var context = new OperationFilterContext(apiDescription, generator, new SchemaRepository(), methodInfo);
            var filter = new SwaggerOperationFilter();

            //act
            filter.Apply(openApiOperation, context);

            //assert
            openApiOperation.OperationId.Should().NotBeNullOrWhiteSpace();
            openApiOperation.OperationId.Should().Be("FakeReadById");
        }

        [Test]
        public void Swagger_Operation_Filter_Should_Not_Assign_Operation_Id_When_No_Entity_Name_Attribute()
        {
            //arrange
            var openApiOperation = new OpenApiOperation { OperationId = nameof(FakeController.ReadByIdAsync).Replace("Async", null) };
            var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ReadByIdAsync));
            var generator = new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new JsonSerializerOptions()));
            var apiDescription = new ApiDescription
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    EndpointMetadata = new List<object>(),
                    ControllerTypeInfo = typeof(FakeController).GetTypeInfo(),
                    ActionName = nameof(FakeController.ReadByIdAsync).Replace("Async", null)
                }
            };
            var context = new OperationFilterContext(apiDescription, generator, new SchemaRepository(), methodInfo);
            var filter = new SwaggerOperationFilter();

            //act
            filter.Apply(openApiOperation, context);

            //assert
            openApiOperation.OperationId.Should().NotBeNullOrWhiteSpace();
            openApiOperation.OperationId.Should().Be("ReadById");
        }

        [Test]
        public void Swagger_Operation_Filter_Should_Sanitize_Entity_Name_From_Operation_Summaries()
        {
            //arrange
            var openApiOperation = new OpenApiOperation
            {
                OperationId = nameof(FakeController.ReadByIdAsync).Replace("Async", null),
                Summary = "Gets a specific {entityName}.",
                Responses = new OpenApiResponses
                {
                    { "200", new OpenApiResponse { Description = "The {entityName} with the given key." } },
                    { "404", new OpenApiResponse { Description = "The {entityName} with the given key is not found." } }
                }
            };
            var methodInfo = typeof(FakeController).GetMethod(nameof(FakeController.ReadByIdAsync));
            var generator = new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new JsonSerializerOptions()));
            var apiDescription = new ApiDescription
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    EndpointMetadata = new List<object> { new OpenApiEntityNameAttribute("Fake", "Fakes") },
                    ControllerTypeInfo = typeof(FakeController).GetTypeInfo(),
                    ActionName = nameof(FakeController.ReadByIdAsync).Replace("Async", null)
                }
            };
            var context = new OperationFilterContext(apiDescription, generator, new SchemaRepository(), methodInfo);
            var filter = new SwaggerOperationFilter();

            //act
            filter.Apply(openApiOperation, context);

            //assert
            openApiOperation.OperationId.Should().NotBeNullOrWhiteSpace();
            openApiOperation.OperationId.Should().Be("FakeReadById");
            openApiOperation.Responses["200"].Description.Should().Be("The Fake with the given key.");
            openApiOperation.Responses["404"].Description.Should().Be("The Fake with the given key is not found.");
        }
    }
}
