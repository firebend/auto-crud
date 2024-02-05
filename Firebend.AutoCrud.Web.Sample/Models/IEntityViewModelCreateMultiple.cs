using Firebend.AutoCrud.Web.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Models;

public interface IEntityViewModelCreateMultiple<T> : IMultipleEntityViewModel<T> where T : IEntityDataAuth;
