using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Models;

public interface IEntityViewModelBase : IEntityDataAuth
{
}

public interface IEntityViewModelCreate<T> where T : IEntityDataAuth
{
    T Body { get; set; }
}

public interface IEntityViewModelCreateMultiple<T> : IMultipleEntityViewModel<T> where T : IEntityDataAuth
{
}

public interface IEntityViewModelRead<TEntity> : IEntity<Guid>, IModifiedEntity
    where TEntity : IEntity<Guid>, IEntityDataAuth
{
}

public interface IEntityViewModelExport : IEntity<Guid>, IModifiedEntity
{
}
