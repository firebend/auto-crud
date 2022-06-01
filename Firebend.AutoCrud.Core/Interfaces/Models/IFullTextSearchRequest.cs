namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface IFullTextSearchRequest : IEntitySearchRequest
{
    /// <summary>
    /// The text string to search by
    /// </summary>
    public string Search { get; set; }
}
