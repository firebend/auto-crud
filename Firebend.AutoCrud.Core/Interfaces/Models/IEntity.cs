namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface IEntity<TKey>
    where TKey : struct
{
    TKey Id { get; set; }
}
