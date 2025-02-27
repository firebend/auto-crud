namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface IActiveEntitySearchRequest
{
    public bool? IsDeleted { get; set; }
}
