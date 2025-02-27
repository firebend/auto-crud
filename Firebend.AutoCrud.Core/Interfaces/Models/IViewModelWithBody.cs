namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface IViewModelWithBody<TBody>
    where TBody : class
{
    public TBody Body { get; set; }
}
