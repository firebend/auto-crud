namespace Firebend.AutoCrud.Core.Interfaces
{
    public interface IEntity<TKey>
        where TKey: struct
    {
        TKey Id { get; set; }
    }
}