using System;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Sample.Models;

namespace Firebend.AutoCrud.EntityFramework.Sample;

public interface IPersonReadRepository : IEntityReadService<Guid, Person>;
