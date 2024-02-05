namespace Firebend.AutoCrud.Web.Sample.Models;

public interface IEntityViewModelCreate<T> where T : IEntityDataAuth
{
    T Body { get; set; }
}
