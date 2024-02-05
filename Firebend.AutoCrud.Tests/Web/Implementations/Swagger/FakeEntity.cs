using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Tests.Web.Implementations.Swagger;

public class FakeEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
}
