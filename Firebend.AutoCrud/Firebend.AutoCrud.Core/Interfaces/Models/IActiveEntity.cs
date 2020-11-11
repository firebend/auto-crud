namespace Firebend.AutoCrud.Core.Interfaces.Models
{
    public interface IActiveEntity
    {
        bool IsDeleted { get; set; }
    }
}
