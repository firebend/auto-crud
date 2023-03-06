namespace Firebend.AutoCrud.Web.Interfaces;

public interface ICopyOnPatchPropertyAccessor<TEntity, TVersion, TViewModel>
{
    public string[] GetProperties();
}
