using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Tests.Mongo.Abstractions.Client.Indexing;

public class FooIndexEntity : IEntity<Guid>, IModifiedEntity
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset ModifiedDate { get; set; }
}
