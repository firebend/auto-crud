namespace Firebend.AutoCrud.Core.Interfaces.Models;

public interface IOrderableSearchRequest : IEntitySearchRequest
{
    /// <summary>
    /// A list of strings representing an order by clause. example ?orderBy=lastName:desc&orderBy=firstName:desc.
    /// </summary>
    public string[] OrderBy { get; set; }
}
