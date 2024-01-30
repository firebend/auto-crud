using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Sample.Models;

public interface IEntityViewModelRead<TKey> : IEntity<TKey>, IModifiedEntity
    where TKey : struct;
