using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Mongo.Sample.Models;

public class Person : IEntity<Guid>//, ITenantEntity<int>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }
    public Guid Id { get; set; }

    public int TenantId { get; set; }
}
