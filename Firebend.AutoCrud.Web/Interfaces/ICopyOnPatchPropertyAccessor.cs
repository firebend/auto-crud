using System.Collections.Generic;

namespace Firebend.AutoCrud.Web.Interfaces;

public interface ICopyOnPatchPropertyAccessor<TEntity, TViewModel>
{
    public string[] GetProperties();
}