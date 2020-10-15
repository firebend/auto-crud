#region

using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

#endregion

namespace Firebend.AutoCrud.Mongo.Sample.Models
{
    public class Person : IEntity<Guid>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public Guid Id { get; set; }
    }
}