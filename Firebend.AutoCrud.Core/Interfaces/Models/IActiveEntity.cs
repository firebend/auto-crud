namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface IActiveEntity
{
    public bool IsDeleted { get; set; }
}
