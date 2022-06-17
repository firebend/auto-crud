using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Sample.Models;

public interface IPerson : IEntity<Guid>, IModifiedEntity, IActiveEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string NickName { get; set; }
}
