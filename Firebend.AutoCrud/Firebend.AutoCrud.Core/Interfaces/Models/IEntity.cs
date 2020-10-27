using System;

namespace Firebend.AutoCrud.Core.Interfaces.Models
{
    public interface IEntity
    {
        
    }
    
    public interface IEntity<TKey> : IEntity
        where TKey : struct
    {
        TKey Id { get; set; }
    }

    public class FooEntity : IEntity<Guid>
    {
        public Guid Id { get; set; }
    }
}