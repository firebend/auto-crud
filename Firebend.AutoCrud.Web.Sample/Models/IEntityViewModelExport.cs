using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Sample.Models;

public interface IEntityViewModelExport : IEntity<Guid>, IModifiedEntity;
