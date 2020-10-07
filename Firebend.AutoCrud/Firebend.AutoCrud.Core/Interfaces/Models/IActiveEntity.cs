namespace Firebend.AutoCrud.Core.Interfaces
{
    public interface IActiveEntity
    {
        bool IsDeleted { get; set; }
    }
}