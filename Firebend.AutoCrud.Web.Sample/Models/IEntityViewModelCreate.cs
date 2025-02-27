namespace Firebend.AutoCrud.Web.Sample.Models;

public interface IEntityViewModelCreate<T> where T : IEntityDataAuth
{
    public T Body { get; set; }
}
