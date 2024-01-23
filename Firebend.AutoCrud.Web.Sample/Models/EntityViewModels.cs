using System;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Models;

public interface IEntityViewModelBase : IEntityDataAuth;

public interface IEntityViewModelCreate<T> where T : IEntityDataAuth
{
    T Body { get; set; }
}

public interface IEntityViewModelCreateMultiple<T> : IMultipleEntityViewModel<T> where T : IEntityDataAuth;

public interface IEntityViewModelRead<TKey> : IEntity<TKey>, IModifiedEntity
    where TKey : struct;

public interface IEntityViewModelExport : IEntity<Guid>, IModifiedEntity;
