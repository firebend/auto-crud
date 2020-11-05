namespace Firebend.AutoCrud.Core.Interfaces.Models
{
    public interface IActiveEntitySearchRequest
    {
        bool? IsDeleted { get; set; }
    }
}