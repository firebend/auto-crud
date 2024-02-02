using System.Collections.Generic;

namespace Firebend.AutoCrud.Web.Interfaces;

public interface ICopyOnPatchPropertyAccessor<TEntity, TVersion, TViewModel>
{
    public ICollection<string> GetProperties();
}
