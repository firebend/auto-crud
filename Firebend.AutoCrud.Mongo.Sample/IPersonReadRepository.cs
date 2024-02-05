using System;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Sample.Models;

namespace Firebend.AutoCrud.Mongo.Sample;

public interface IPersonReadRepository : IEntityReadService<Guid, Person>;
