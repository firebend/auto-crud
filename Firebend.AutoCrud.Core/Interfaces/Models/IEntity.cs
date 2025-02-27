namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface IEntity<TKey>
    where TKey : struct
{
    public TKey Id { get; set; }
}
