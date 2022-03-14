namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface IEntitySearchRequest
{
    /// <summary>
    /// The starting page number for this search request.
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// The total number of records per page
    /// </summary>
    public int? PageSize { get; set; }
}
