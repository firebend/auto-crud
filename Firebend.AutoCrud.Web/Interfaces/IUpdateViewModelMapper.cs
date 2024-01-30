using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Web.Interfaces;

public interface IUpdateViewModelMapper<TKey, TEntity, TVersion, TViewModel> : IViewModelMapper<TKey, TEntity, TVersion, TViewModel>
    where TEntity : IEntity<TKey>
    where TKey : struct
    where TVersion : class, IAutoCrudApiVersion
    where TViewModel : class;
