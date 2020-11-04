using System;

namespace Firebend.AutoCrud.Core.Interfaces.Models
{
    public interface IModifiedEntity
    {
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
    }
}