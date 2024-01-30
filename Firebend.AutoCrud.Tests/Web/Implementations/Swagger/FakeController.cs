using System;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Firebend.AutoCrud.Tests.Web.Implementations.Swagger;

public class FakeController : AbstractEntityReadController<Guid, FakeEntity, V1, FakeEntity>
{
    public FakeController(IEntityReadService<Guid, FakeEntity> readService,
        IEntityKeyParser<Guid, FakeEntity, V1> entityKeyParser,
        IReadViewModelMapper<Guid, FakeEntity, V1, FakeEntity> viewModelMapper,
        IOptions<ApiBehaviorOptions> apiOptions) : base(readService, entityKeyParser, viewModelMapper, apiOptions)
    {
    }
}
