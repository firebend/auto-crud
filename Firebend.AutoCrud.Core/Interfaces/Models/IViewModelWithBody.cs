namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface IViewModelWithBody<TBody>
    where TBody : class
{
    TBody Body { get; set; }
}
