
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Models.CustomFields;

public class CustomFieldsSearchRequest : IEntitySearchRequest
{
    /// <summary>
    /// The starting page number for this search request.
    /// </summary>
    public int? PageNumber { get; set; }

    /// <summary>
    /// The total number of records per page
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Key to search by
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Value to search by
    /// </summary>
    public string Value { get; set; }

    /// <inheritdoc />
    public bool? DoCount { get; set; }
}
