using System;

namespace Firebend.AutoCrud.Core.Interfaces.Models;

public class FooEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
}
